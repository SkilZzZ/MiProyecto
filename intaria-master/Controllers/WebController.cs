using Intaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using qDatabase;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Intaria.Controllers
{
    public class WebController : Controller
    {
        private readonly IConfiguration _config;
        private readonly Database _db;
        private readonly IWebHostEnvironment _env;
        private readonly GoogleConfigModel _googleConfig;

        
        public WebController(IWebHostEnvironment env, 
                             IOptions<GoogleConfigModel> googleConfig,
                             IConfiguration config,
                             Database db) : base()
        {
            _googleConfig = googleConfig.Value;
            _env = env;
            _config = config;
            _db = db;
        }

        /// <summary>
        /// Pagina Principal
        /// </summary>
        public IActionResult Index()
        {
            
            //En esta página sólo cargamos las categorias y el carrusel,
            //todo lo demás se carga de forma dinámica utilizando ajax.
            ViewBag.categorias = ordernarLista(Guid.Empty, _db.GetRecords<Categoria>("select distinct(C.nombre),C.* from Categorias C  inner join Articulos A on C.Id=A.IdCategoria" +
                " where A.Estado='en venta'"));
            ViewBag.listaFotosCarousel = _db.GetRecords<Carousel>("SELECT * FROM [Carousel]");
            ViewBag.Listamenucategorias = _db.GetRecords<Categoria>("select * from categorias");

            return View();
        }





        #region Articulos
        [Route("/articulo/{id?}")]
        [HttpGet]

        public ViewResult Articulo(string id, string q)
        {
            /// ID= ADDSAFASDF-1234
            
            var ultcar = id.Substring(id.Length - 4, 4);
            var articulo = _db.GetRecords<Articulo>("SELECT * FROM [articulos] WHERE [Id] like '" + ultcar.Replace("'", "''") + "%'").FirstOrDefault();

            if (articulo == null)
            {
                articulo = new Articulo();

            }

            var listaFotos = _db.GetRecords<Fotos>("SELECT * FROM [Fotos] WHERE [IdArticulo] like '" + ultcar.Replace("'", "''") + "%'");

            ViewBag.listaFotos = listaFotos;

            var listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");

            ViewBag.listaCategoria = listaCategoria;

            return View(articulo);

        }


        public ActionResult categoriasmenuuu()
        {
            

            var Listacategorias = _db.GetRecords<Categoria>(@"select * from categorias");

            ViewBag.listaCategoria = ordernarLista(Guid.Empty, _db.GetRecords<Categoria>("select distinct(C.nombre),C.* from Categorias C  inner join Articulos A on C.Id=A.IdCategoria"));

            ViewBag.Listamenucategorias = _db.GetRecords<Categoria>("select * from categorias");

            ViewBag.categorias = ordernarLista(Guid.Empty, _db.GetRecords<Categoria>("select distinct(C.nombre),C.* from Categorias C  inner join Articulos A on C.Id=A.IdCategoria"));


            return PartialView("/Views/Web/prueba/_categoriasmenu.cshtml", Listacategorias);

        }

        public PartialViewResult Articulos(string otrapagina, string numarticulos, string categoria, string q, int pagina = 0)
        {

            
            const int ppp = 15;

            int offset = ppp * pagina;

            var articulos = _db.GetRecords<Articulo>(@"with asd as  (SELECT ID  FROM [Categorias]  where id ='" + categoria + "' union all select child.ID  from" +
                " categorias as child join asd as parent on parent.id = child.CategoriaPadre  ) select A.*,  F.NombreArchivo from [Articulos] A" +
                    " OUTER APPLY(SELECT TOP 1 * FROM [Fotos] F WHERE F.IdArticulo = A.Id  order by [CreatedOn] desc) F WHERE A.idcategoria in(select * from asd)"
                        + (!string.IsNullOrEmpty(q) ? " AND A.Nombre like '%" + q.Replace("'", "''") + "%'" : "") +
                             " ORDER BY [CreatedOn] DESC offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");



            ViewBag.cate = _db.GetRecords<Articulo>(@"select A.*,  F.NombreArchivo from [Articulos] A
                       OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F  where idcategoria in (SELECT ID  FROM [Categorias]  where id ='" + categoria + "')");


            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias] where id='" + categoria + "'");


            var totalArticulos = Convert.ToInt32(_db.ExecuteScalar("with asd as  (SELECT ID  FROM [Categorias]  where id ='" + categoria + "' union all select child.ID  from" +
                " categorias as child join asd as parent on parent.id = child.CategoriaPadre  )" +
                "select count(id) from articulos where idcategoria in (select * from asd)"));

            ViewBag.busqueda = q;

            ViewBag.totalArticulos = totalArticulos;
            ViewBag.totalPagina = Math.Ceiling((double)totalArticulos / ppp);
            ViewBag.pagina = pagina;

            ViewBag.listaFotos = _db.GetRecords<Fotos>("SELECT * FROM [Fotos]");


            //HttpContext.Session.Set<List<Articulo>>("CARRITO", new List<Articulo>());


            return PartialView("/Views/Web/Partial/_Articulos.cshtml", articulos);
        }

        public PartialViewResult BuscarArticulosResultado(int otrapagina, string ordenadorpor, int numarticulos, string q, string vainputcat, int pagina)
        {

            

            var order = "[CreatedOn] DESC ";

            if (ordenadorpor == "older")
            {
                order = " [CreatedOn] ASC ";

            }
            else if (ordenadorpor == "low")
            {
                order = " [Precio] asc ";
            }
            else if (ordenadorpor == "hight")
            {
                order = " [Precio] DESC ";
            }

            int ppp = 15;

            if (numarticulos != 0)
            {
                ppp = numarticulos;
            }

            if (otrapagina != 0)
            {
                pagina = otrapagina - 1;
            }
            int offset = ppp * pagina;





            var articulos = _db.GetRecords<Articulo>(@"" + "with asd as (SELECT id  FROM[Categorias] where Nombre = '" + vainputcat + "' union all select child.ID  from categorias as child" +
                                                     " join asd as parent on parent.id = child.CategoriaPadre )" +
                                                   "select A.*,  F.NombreArchivo from [Articulos] A OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id   order by [CreatedOn] desc) F  "
                                                 + (!string.IsNullOrEmpty(vainputcat) ? " WHERE A.idcategoria in (select id from asd) " : "")
                                               + (!string.IsNullOrEmpty(q) ? " and A.Nombre like '%" + q.Replace("'", "''") + "%'" : "") +
                                              " ORDER BY " + order + " offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");




            var totalArticulos = Convert.ToInt32(_db.ExecuteScalar("with asd as  (SELECT ID  FROM [Categorias]  where nombre ='" + vainputcat + "' union all select child.ID  from" +
                " categorias as child join asd as parent on parent.id = child.CategoriaPadre  )" +
                "select count(id) from articulos where idcategoria in (select * from asd) "));

            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");

            ViewBag.totalArticulos = totalArticulos;
            ViewBag.totalPagina = Math.Ceiling((double)totalArticulos / ppp);
            ViewBag.pagina = pagina;



            return PartialView("/Views/web/webmodal/_resultadobusqueda.cshtml", articulos);


        }


        [HttpPost]
        public JsonResult AnadirAlCarrito(Guid Id, string Remove)
        {
            
            var carrito = HttpContext.Session.Get<List<Guid>>("CARRITO");
            if (carrito == null || carrito.Count == 0)
            {
                carrito = new List<Guid>();
            }
            if (Remove == "si")
            {
                carrito.Remove(Id);
            }
            else
            {
                carrito.Add(Id);
            }

            HttpContext.Session.Set<List<Guid>>("CARRITO", carrito);

            return new JsonResult(new { V = "OK" });
        }

        [Route("cart")]
        public IActionResult Cesta()
        {
            
            var carrito = HttpContext.Session.Get<List<Guid>>("CARRITO");
            if (carrito == null || carrito.Count == 0)
            {
                return View(new List<Articulo>());
            }

            var articulos = _db.GetRecords<Articulo>("select A.*,  F.NombreArchivo from [Articulos] A" +
                                                        " OUTER APPLY(SELECT TOP 1 * FROM [Fotos] F WHERE F.IdArticulo = A.Id) F  where A.Id in ('" + string.Join("','", carrito) + "')");

            ViewBag.articulos = articulos;

            ViewBag.ultimoano = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN YEAR(GETDATE()) -1 AND GETDATE()");
            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");


            ViewBag.listaFotos = _db.GetRecords<Fotos>("select * from fotos");


            return View(articulos);
        }


        [Authorize]
        [Route("payment")]
        public IActionResult PagoyEnvio()
        {
            
            var carrito = HttpContext.Session.Get<List<Guid>>("CARRITO");
            if (carrito == null || carrito.Count == 0)
            {
                return View(new List<Articulo>());
            }


            ViewBag.EmailUsuariologeado = _db.ExecuteScalar("SELECT email FROM [Usuarios] WHERE [Nombre] = '" + User.Identity.Name + "'");


            var articulos = _db.GetRecords<Articulo>("select A.*,  F.NombreArchivo from [Articulos] A" +
                                                        " OUTER APPLY(SELECT TOP 1 * FROM [Fotos] F WHERE F.IdArticulo = A.Id) F  where A.Id in ('" + string.Join("','", carrito) + "')");

            ViewBag.articulos = articulos;


            ViewBag.ultimoano = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN YEAR(GETDATE()) -1 AND GETDATE()");
            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");


            ViewBag.infousuario = _db.GetRecords<Usuarios>("select * from usuarios where nombre='" + User.Identity.Name + "'");


            ViewBag.listaFotos = _db.GetRecords<Fotos>("select * from fotos");



            return View(articulos);
        }

        [HttpPost]
        [Route("payment")]
        public IActionResult PagoyEnvio(PaymodelView data)
        {
            
            if (data.TipoPago == "tarjeta")
            {

                var customers = new CustomerService();
                var charges = new ChargeService();

                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = data.Email,
                    Source = data.Token
                });

                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(data.Total),
                    Description = "Test Payment",
                    Currency = "EUR",
                    Customer = customer.Id,
                    ReceiptEmail = "asd@gmail.com",

                });

                if (charge.Status == "succeeded")
                {
                    BalanceTransaction BalanceTransactionId = charge.BalanceTransaction;
                    return Redirect("myaccount");

                }
                else
                {
                    BalanceTransaction BalanceTransactionId = charge.BalanceTransaction;
                    return View();

                }

            }
            else
            {
                return View();

            }

        }



        public List<JsonTree> ordernarLista(Guid Id, List<Categoria> lista)
        {
             
            var tmp = new List<JsonTree>();
            foreach (var c in lista.Where(W => W.CategoriaPadre == Id))
            {
                tmp.Add(new JsonTree()
                {
                    id = c.Id.ToString(),
                    text = c.Nombre,
                    nodes = ordernarLista(c.Id, lista)
                });
            }

            return tmp.Count > 0 ? tmp : null;
        }

        #endregion

        [Route("Termsofsale")]
        public IActionResult ConditionsSale()
        {


            return View();
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {

            return View();
        }

        [Route("contact")]
        public IActionResult Contacto()
        {
             
            ViewBag.EmailUsuariologeado = _db.ExecuteScalar("SELECT email FROM [Usuarios] WHERE [Nombre] = '" + User.Identity.Name + "'");
            ViewBag.telefonoUsuariologeado = _db.ExecuteScalar("SELECT telefono FROM [Usuarios] WHERE [Nombre] = '" + User.Identity.Name + "'");


            return View();
        }


        [Route("About-us")]
        public IActionResult Quienessomos()
        {
            return View();

        }

        [Route("Contact")]

        [HttpPost]
        public IActionResult Contact(LoginViewModel LoginViewModel)
        {
             
            if (ModelState.IsValid)
            {
                string EmailOrigen = "intariamilitaria@compramosmedallasycondecoraciones.es";
                string EmailDestino = "intariamilitaria@compramosmedallasycondecoraciones.es";
                string Contraseña = "ionosmierda";


                var mensaje = "Nombre: " + LoginViewModel.Name + " <br> Email: " + LoginViewModel.Email + "  <br> tfno:" + LoginViewModel.Phone + "<br><br>" + LoginViewModel.Message + "<br><br><br>captcha" + LoginViewModel.Token;
                MailMessage omailMessage = new MailMessage(EmailOrigen, EmailDestino, "Contacto pagina", mensaje);


                foreach (var file in Request.Form.Files)
                {
                    var fileStream = new MemoryStream();
                    file.CopyTo(fileStream);
                    omailMessage.Attachments.Add(new Attachment(new MemoryStream(fileStream.ToArray()), file.FileName, file.ContentType));
                }

                omailMessage.IsBodyHtml = true;

                SmtpClient oSmtpClient = new SmtpClient("smtp.ionos.es");
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                oSmtpClient.EnableSsl = true;
                oSmtpClient.Port = 587;
                oSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                oSmtpClient.UseDefaultCredentials = false;
                oSmtpClient.Credentials = new System.Net.NetworkCredential(EmailOrigen, Contraseña);
                oSmtpClient.Send(omailMessage);
                oSmtpClient.Dispose();

                ViewBag.ClearFields = true;



            }
            return View();
        }

        private bool IsReCaptchValidV3(string captchaResponse)
        {
            
            var result = false;
            var secretKey = _googleConfig.Secret;
            var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
            var requestUri = string.Format(apiUrl, secretKey, captchaResponse);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    JObject jResponse = JObject.Parse(stream.ReadToEnd());
                    var isSuccess = jResponse.Value<bool>("success");
                    result = (isSuccess) ? true : false;
                }
            }
            return result;
        }

        public IActionResult ProcessFile()
        {
            return View();
        }





        [Route("Myaccount")]
        [HttpGet]
        [Authorize]
        public IActionResult MiCuenta(string valpost)
        {
            
            ViewBag.existeclave = Convert.ToString(_db.ExecuteScalar("SELECT Clave FROM [Usuarios] WHERE [Nombre] = '" + User.Identity.Name + "'"));


            var ListaUsuarios = _db.GetRecords<Usuarios>("SELECT * FROM [Usuarios] WHERE [Nombre] = '" + User.Identity.Name + "'").FirstOrDefault();


            if (ListaUsuarios == null)
            {
                ListaUsuarios = new Usuarios();
            }

            ViewBag.totalListapedidos = _db.ExecuteScalar("SELECT count(id) FROM [Pedidos] where idUsuario in (select UserId from usuarios where nombre='" + User.Identity.Name + "') ");

            ViewBag.Listapedidos = _db.GetRecords<Pedidos>("SELECT * FROM [intaria].[dbo].[Pedidos] where idUsuario in (select UserId from usuarios where nombre='" + User.Identity.Name + "')   order by FechaPago desc");
            ViewBag.listaPedidos_Articulos = _db.GetRecords<Pedidos_Articulos>(@"SELECT P.*, PA.* FROM [pedidos] P left join  [Pedidos_Articulos] PA
                                                on P.Id= PA.IdPedido where P.IdUsuario in (select UserId from Usuarios where nombre='" + User.Identity.Name + "')");

            ViewBag.listaArticulos = _db.GetRecords<Articulo>(@"select A.*,  F.* from [Articulos] A OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F where A.id in (select idarticulo from Pedidos_articulos)");

            ViewBag.listaFotos = _db.GetRecords<Fotos>(@"select A.*,  F.* from [Articulos] A OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F");



            return View(ListaUsuarios);
        }

        [Route("Myaccount")]

        [HttpPost]
        public IActionResult MiCuenta(string personaldates, string email, string UserId, int Telefono, string pais, string Provincia, string Localidad, string Direccion, string OtraDireccion, int CodigoPostal, string changepass, string createpass, string currentpassword, string newpassword, string repeatpassword)
        {
            
            if (personaldates == "personaldates")
            {

                _db.ExecuteSQL("UPDATE [Usuarios] SET  [Telefono] = @telefono, [Pais]=@pais,[Provincia]=@provincia,[Localidad]=@localidad,[Direccion1]=@direccion,[Direccion2]=@Otradireccion,[CodigoPostal]=@codigopostal  WHERE [UserId] = @userid or [Email] = @email",
                      new Dictionary<string, object>() {
                        {"@telefono", Telefono },
                        {"@pais", pais},
                        {"@provincia", Provincia},
                        {"@localidad", Localidad},
                        {"@direccion", Direccion},
                        {"@Otradireccion", OtraDireccion},
                        {"@codigopostal", CodigoPostal},
                        {"@userid", UserId },
                        {"@email",email }
                        });

            }
            if (changepass == "changepass")
            {



                var comprobarclave = Convert.ToString(_db.ExecuteScalar("select Clave from Usuarios   WHERE [UserId] = @userid",
                   new Dictionary<string, object>() {
                      {"@userid", UserId }
                   }));

                if (currentpassword != comprobarclave && comprobarclave != "")
                {
                    ViewBag.error = "the current password is not correct";
                    return MiCuenta(null);
                }
                else if (newpassword != repeatpassword)
                {
                    ViewBag.error = "Passwords do not match";
                    return MiCuenta(null);

                }
                else if (newpassword == null)
                {
                    ViewBag.error = "no field can be left blank";
                    return MiCuenta(null);

                }
                else if (newpassword == repeatpassword)
                {

                    _db.ExecuteSQL("UPDATE [Usuarios] SET  [clave] = @clave WHERE [UserId] = @userid",
                     new Dictionary<string, object>() {
                    {"@clave",newpassword },
                    {"@userid", UserId }
                    });
                    ViewBag.Message = "password changed successfully";
                    return MiCuenta(null);


                }
            }

            return MiCuenta(personaldates);
        }


        [HttpPost]
        public JsonResult completadopago(string email, string articulos, string cantidad, decimal totalpedido)
        {
            
            Response.Redirect("prueba");
            var carrito = HttpContext.Session.Get<List<Guid>>("CARRITO");
            int cantidadinsertada = 0;
            int contador = 0;
            var compCantArt = 0;

            var IdUsuario = _db.ExecuteScalar(@"select TOP 1 [UserId] from usuarios where email ='" + email + "'");

            ViewBag.Total = totalpedido;



            List<string> idArticulos = new List<string>();
            List<string> idCantidad = new List<string>();

            if (!string.IsNullOrEmpty(articulos))
            {
                idArticulos = articulos.Split(',').ToList();


            }

            if (!string.IsNullOrEmpty(cantidad))
            {
                idCantidad = cantidad.Split(',').ToList();
            }

            if (articulos != null)
            {



                _db.ExecuteSQL("INSERT INTO [Pedidos]([Estado], [FormaPago],[IdUsuario],[Total],[CreadoPorMi]) values ('Vendido', 'tarjeta',@idusuario,@total,'NOO')", new Dictionary<string, object>() {
                    { "@idusuario",IdUsuario},
                    { "@total", totalpedido }
                });

                var Id = _db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Pedidos] ORDER BY [CreatedOn] DESC").ToString();



                foreach (var idArticulo in idArticulos)
                {
                    cantidadinsertada = Convert.ToInt32(idCantidad[contador]);
                    contador = contador + 1;

                    //TODO completar la query
                    _db.ExecuteSQL("insert into [pedidos_articulos] (idpedido, idarticulo) values (@id, @articulo)", new Dictionary<string, object>() {
                    {"@id", Id },
                    {"@articulo", idArticulo }
                    });



                    _db.ExecuteSQL("update Articulos set Cantidad=Cantidad-" + cantidadinsertada + " where Id='" + idArticulo + "'");

                    compCantArt = Convert.ToInt32(_db.ExecuteScalar("select Cantidad from Articulos where Id='" + idArticulo + "'"));

                    if (compCantArt <= 0)
                    {
                        _db.ExecuteSQL("update Articulos set Estado='Vendido' where Id='" + idArticulo + "'");

                    }



                }
            }


            return new JsonResult(new { V = "OK" });

        }






    }

}
