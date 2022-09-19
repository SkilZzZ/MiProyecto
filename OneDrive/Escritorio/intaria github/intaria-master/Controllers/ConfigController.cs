using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using qDatabase;










namespace Intaria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConfigController : Controller
    {
        // public Database _db = new Database("Server=intariam.database.windows.net;Database=intaria;User ID=SAINTARIA; Password=intariamilitariA!12");
        public Database _db = new Database("Server = localhost; Database=intaria;Trusted_Connection=True;");
        private readonly IWebHostEnvironment _env;


        public ConfigController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Route("config")]

        #region index
        public IActionResult Index()
        {

            var Totalpedidos = _db.GetRecords<Pedidos>("SELECT * FROM Pedidos");

            ViewBag.ultimas24 = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN GETDATE() -1 AND GETDATE()");
            ViewBag.ultimasemana = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN GETDATE() -7 AND GETDATE()");
            ViewBag.ultimomes = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN GETDATE() -30 AND GETDATE()");
            ViewBag.ultimoano = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN YEAR(GETDATE()) -1 AND GETDATE()");


            ViewBag.ase = _db.ExecuteScalar("select sum(Total) from Pedidos where Estado in (select Estado from pedidos where NOT Estado = 'PosibleVenta') and FechaPago BETWEEN YEAR(GETDATE()) -2 AND GETDATE()");


            return View(Totalpedidos);
        }

        #endregion

        #region categorias


        [HttpPost]
        public IActionResult Categorias(string Id, string q)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _db.ExecuteSQL("delete  FROM [categorias] where [id]= @Id", new Dictionary<string, object>() {
                { "@Id", Id}
            });

            }

            return Categorias(q);
        }

        [HttpGet]
        public IActionResult Categorias(string q, int pagina = 0)
        {

            const int ppp = 6;
            int offset = ppp * pagina;


            var Categorias = _db.GetRecords<Categoria>(@"select C.* from [Categorias] C "
                                                + (!string.IsNullOrEmpty(q) ? "WHERE C.Nombre like '%" + q.Replace("'", "''") + "%'" : "") +
                                                    "ORDER BY [CreatedOn] DESC offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");

            var totalCategorias = Convert.ToInt32(_db.ExecuteScalar("select count(id) from Categorias"));

            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");
            ViewBag.totalCategorias = totalCategorias;
            ViewBag.totalPagina = Math.Ceiling((double)totalCategorias / ppp);
            ViewBag.pagina = pagina;
            ViewBag.busqueda = q;

            return View(Categorias);

        }

        [HttpGet]
        public IActionResult CategoriasEdit(string Id)
        {

            var categoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias] WHERE [Id] = @Id",
                new Dictionary<string, object>() {
              { "@Id", Id}
             }).FirstOrDefault();

            var listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");

            ViewBag.listaCategoria = ordernarLista("", Guid.Empty, listaCategoria);


            if (categoria == null)
            {
                categoria = new Categoria();
            }


            return View(categoria);
        }





        [HttpPost]
        public IActionResult CategoriasEdit(string Id, string nombre, string descripcion, string categoriaPadre)
        {

            if (string.IsNullOrEmpty(Id) || Guid.Parse(Id) == Guid.Empty)
            {

                _db.ExecuteSQL("INSERT INTO [Categorias] ([Nombre], [Descripcion], [CategoriaPadre]) VALUES (@Nombre, @Descripcion, @CategoriaPadre)", new Dictionary<string, object>() {
                    { "@Nombre", nombre},
                    { "@Descripcion", descripcion },
                    { "@CategoriaPadre", categoriaPadre }
                });
                Id = _db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Categorias] ORDER BY [CreatedOn] DESC").ToString();

                var idcreada = Convert.ToString(_db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Categorias] ORDER BY [CreatedOn] DESC"));
                Response.Redirect("/config/CategoriasEdit/" + idcreada);
            }
            else
            {

                _db.ExecuteSQL("UPDATE [Categorias] SET [Nombre] = @Nombre, [Descripcion] = @Descripcion, [CategoriaPadre] = @CategoriaPadre WHERE [Id] = @Id", new Dictionary<string, object>() {
                { "@Nombre", nombre},
                { "@Descripcion", descripcion },
                { "@CategoriaPadre", categoriaPadre },
                { "@Id", Id}
            });

            }




            return CategoriasEdit(Id);
        }
        #endregion

        #region articulos


        [HttpPost]
        public IActionResult Articulos(string Id, string q)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _db.ExecuteSQL("delete  FROM [fotos] where [IdArticulo]= @Id", new Dictionary<string, object>() {
                    { "@Id", Id}
                });

                _db.ExecuteSQL("delete  FROM [articulos] where [id]= @Id", new Dictionary<string, object>() {
                    { "@Id", Id}
                });

            }
            return Articulos(q);
        }

        public IActionResult Articulos(string q, int pagina = 0)
        {
            const int ppp = 5;
            int offset = ppp * pagina;

            var articulos = _db.GetRecords<Articulo>(@"select A.*,  F.NombreArchivo from [Articulos] A
                                                        OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F  "
                                                + (!string.IsNullOrEmpty(q) ? "WHERE A.Nombre like '%" + q.Replace("'", "''") + "%'" : "") +
                                                    " ORDER BY [CreatedOn] DESC offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");

            var totalArticulos = Convert.ToInt32(_db.ExecuteScalar("select count(id) from articulos where Estado='en venta'"));


            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");
            ViewBag.busqueda = q;





            ViewBag.totalArticulos = totalArticulos;
            ViewBag.totalPagina = Math.Ceiling((double)totalArticulos / ppp);
            ViewBag.pagina = pagina;


            return View(articulos);
        }





        [HttpGet]
        public IActionResult ArticulosEdit(String Id)
        {

            var articulo = _db.GetRecords<Articulo>("SELECT * FROM [articulos] WHERE [Id] = @Id",
                new Dictionary<string, object>() {
                { "@Id", Id}
            }).FirstOrDefault();

            var listaFotos = _db.GetRecords<Fotos>("SELECT * FROM [Fotos] WHERE [IdArticulo] = @IdArticulo order by [CreatedOn] DESC",
                new Dictionary<string, object>() {
                { "@IdArticulo", Id}
            });

            ViewBag.listaFotos = listaFotos;

            var listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");

            //  var listaCategoriaEbay = _db.GetRecords<CategoriasEbay>("SELECT * FROM [categoriasEbay]");


            //  ViewBag.listaCategoriaEbay = ordernarListaCategoria("", Guid.Empty, listaCategoriaEbay);

            ViewBag.listaCategoria = ordernarLista("", Guid.Empty, listaCategoria);

            if (articulo == null)
            {
                articulo = new Articulo();
            }

            return View(articulo);

        }


        [HttpPost]
        public IActionResult ArticulosEdit(string Id, string nombre, string descripcion, string IdCategoria, int Precio, int Cantidad)
        {
            if (string.IsNullOrEmpty(Id) || Guid.Parse(Id) == Guid.Empty)
            {
                _db.ExecuteSQL("INSERT INTO [Articulos] ([Nombre], [Descripcion], [IdCategoria],[Precio],[Cantidad], [Estado]) VALUES (@Nombre, @Descripcion, @IdCategoria,@precio,@cantidad, @estado)", new Dictionary<string, object>() {
                    { "@Nombre", nombre},
                    { "@Descripcion", descripcion },
                    { "@IdCategoria", IdCategoria },
                    { "@precio", Precio },
                    {"@cantidad",Cantidad },
                    {"@estado", "en venta" }

                });
                Id = _db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Articulos] ORDER BY [CreatedOn] DESC").ToString();

                var idcreada = Convert.ToString(_db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Articulos] ORDER BY [CreatedOn] DESC"));
                Response.Redirect("/config/ArticulosEdit/" + idcreada);
            }
            else
            {

                _db.ExecuteSQL("UPDATE [Articulos] SET [Nombre] = @Nombre, [Descripcion] = @Descripcion, [IdCategoria] = @IdCategoria,[Precio]=@precio,[Cantidad]=@cantidad WHERE [Id] = @Id", new Dictionary<string, object>() {
                    { "@Nombre", nombre},
                    { "@Descripcion", descripcion },
                    { "@IdCategoria", IdCategoria },
                    { "@Id", Id},
                    { "@precio", Precio},
                    {"@cantidad",Cantidad }


                });

            }

            foreach (var file in Request.Form.Files)
            {


                _db.ExecuteSQL("INSERT INTO [Fotos] ([IdArticulo], [NombreArchivo]) VALUES (@IdArticulo, @nombreArchivo)", new Dictionary<string, object>() {
                    { "@IdArticulo", Id },
                    { "@nombreArchivo", file.FileName }
                });

                if (!System.IO.Directory.Exists(_env.WebRootPath + "/fotos/" + Id))
                {
                    System.IO.Directory.CreateDirectory(_env.WebRootPath + "/fotos/" + Id);
                }

                using (var fileStream = new System.IO.FileStream(_env.WebRootPath + "/fotos/" + Id + "/" + file.FileName, System.IO.FileMode.Create))
                    file.CopyTo(fileStream);
            }

            return ArticulosEdit(Id);

        }




        [HttpGet]
        public IActionResult BorrarFoto(String Id)
        {

            var foto = _db.GetRecords<Fotos>("SELECT * FROM FOTOS WHERE Id = @Id", new Dictionary<string, object>() {
                { "@Id", Id}
            }).First();

            _db.ExecuteSQL("delete FROM [Fotos] where [id]= @Id", new Dictionary<string, object>() {
                { "@Id", Id}
            });

            return Redirect("/config/articulosedit/" + foto.IdArticulo.ToString());

        }




        #endregion

        #region funcion ordenarLista


        public List<Categoria> ordernarLista(string guion, Guid Id, List<Categoria> lista)
        {
            var tmp = new List<Categoria>();
            foreach (var c in lista.Where(W => W.CategoriaPadre == Id))
            {
                c.Nombre = guion + " " + c.Nombre;
                tmp.Add(c);
                tmp.AddRange(ordernarLista(guion + "-", c.Id, lista));
            }

            return tmp;
        }

        public List<CategoriasEbay> ordernarListaCategoria(string guion, Guid Id, List<CategoriasEbay> lista)
        {
            var tmp = new List<CategoriasEbay>();
            foreach (var c in lista.Where(W => W.CategoriaPadre == Id))
            {
                c.Nombre = guion + " " + c.Nombre;
                tmp.Add(c);
                tmp.AddRange(ordernarListaCategoria(guion + "-", c.Id, lista));
            }

            return tmp;
        }

        #endregion

        #region clientes

        [HttpPost]
        public IActionResult Clientes(string UserId, string q)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                _db.ExecuteSQL("delete  FROM [pedidos] where [IdUsuario]= @userid", new Dictionary<string, object>() {
                { "@userid", UserId}
            });

                _db.ExecuteSQL("delete  FROM [Usuarios] where [UserId]= @userid", new Dictionary<string, object>() {
                { "@userid", UserId}
            });

            }

            return Clientes(q);
        }

        public IActionResult Clientes(string q, int pagina = 0)
        {

            const int ppp = 5;
            int offset = ppp * pagina;


            var ListaUsuarios = _db.GetRecords<Usuarios>(@"select U.* from [Usuarios] U OUTER APPLY(SELECT TOP 1 * FROM[pedidos] P WHERE P.Idusuario = U.UserId) P  "
                                                + (!string.IsNullOrEmpty(q) ? "WHERE U.Nombre like '%" + q.Replace("'", "''") + "%'" : "") +
                                                    "ORDER BY [CreatedOn] DESC offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");


            ViewBag.busqueda = q;



            var CuentaUsuarios = Convert.ToInt32(_db.ExecuteScalar("select count(id) from articulos"));


            ViewBag.CuentaUsuarios = CuentaUsuarios;
            ViewBag.totalPagina = Math.Ceiling((double)CuentaUsuarios / ppp);
            ViewBag.pagina = pagina;

            return View(ListaUsuarios);
        }




        [HttpGet]
        public IActionResult ClientesEdit(string Id)
        {

            var ListaUsuarios = _db.GetRecords<Usuarios>("SELECT * FROM [Usuarios] WHERE [UserId] = @UserId",
                new Dictionary<string, object>() {
                { "@UserId", Id}
            }).FirstOrDefault();

            if (ListaUsuarios == null)
            {
                ListaUsuarios = new Usuarios();
            }

            return View(ListaUsuarios);
        }



        [HttpPost]
        public IActionResult ClientesEdit(string UserId, string Nombre, string Email, int Telefono, string pais, string Provincia, string Localidad, string Direccion, string OtraDireccion, int CodigoPostal)
        {
            if (string.IsNullOrEmpty(UserId) || Guid.Parse(UserId) == Guid.Empty)
            {
                var existe = _db.ExecuteScalar("SELECT UserId FROM [usuarios] WHERE [Email] = @email", new Dictionary<string, object>() { { "@email", Email } });
                if (existe != null)
                {
                    //el email ya existe en la bbdd
                    ViewBag.error = "El Email ya existe";
                    return ClientesEdit(null);
                }

                _db.ExecuteSQL("INSERT INTO [usuarios] ([Nombre],[Email],[telefono],[Pais],[Provincia],[Localidad],[Direccion1],[Direccion2],[CodigoPostal],[CreadoPorMi]) values (@nombre, @email,@telefono,@pais,@provincia,@localidad,@direccion,@Otradireccion,@codigopostal,'si')",
                        new Dictionary<string, object>()
                    {
                        {"@nombre", Nombre },
                        {"@email",Email},
                        {"@telefono", Telefono },
                        {"@pais", pais},
                        {"@provincia", Provincia},
                        {"@localidad", Localidad},
                        {"@direccion", Direccion},
                        {"@Otradireccion", OtraDireccion},
                        {"@codigopostal", CodigoPostal},
                    });

                var idcreada = Convert.ToString(_db.ExecuteScalar("SELECT top 1 UserId FROM [Usuarios]  ORDER BY [CreatedOn] DESC"));
                Response.Redirect("/config/ClientesEdit/" + idcreada);
            }
            else
            {

                _db.ExecuteSQL("UPDATE [Usuarios] SET [Nombre] = @Nombre, [Email] = @email, [Telefono] = @telefono, [Pais]=@pais,[Provincia]=@provincia,[Localidad]=@localidad,[Direccion1]=@direccion,[Direccion2]=@Otradireccion,[CodigoPostal]=@codigopostal  WHERE [UserId] = @userid",
                    new Dictionary<string, object>() {
                        {"@nombre", Nombre },
                        {"@email",Email},
                        {"@telefono", Telefono },
                        {"@pais", pais},
                        {"@provincia", Provincia},
                        {"@localidad", Localidad},
                        {"@direccion", Direccion},
                        {"@Otradireccion", OtraDireccion},
                        {"@codigopostal", CodigoPostal},
                        {"@userid", UserId }
                    });
            }




            return ClientesEdit(UserId);
        }

        #endregion

        #region pedidos




        [HttpPost]
        public IActionResult Pedidos(string Id, string q)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _db.ExecuteSQL("delete  FROM [Pedidos] where [id]= @Id", new Dictionary<string, object>() {
                { "@Id", Id}
            });

            }
            return Pedidos(q);
        }

        public IActionResult Pedidos(string q, int pagina = 0)
        {
            const int ppp = 5;
            int offset = ppp * pagina;

            var ListaPedidos = _db.GetRecords<Pedidos>("select P.*, U.Nombre as [NombreUsuario] from [Pedidos] P inner join [Usuarios] U on U.UserID = P.IdUsuario "
             + (!string.IsNullOrEmpty(q) ? "WHERE P.Estado like '%" + q.Replace("'", "''") + "%'" : "") +
            " ORDER BY [CreatedOn] desc offset " + offset + " rows fetch next " + ppp + " ROWS ONLY");

            var totalPedidos = Convert.ToInt32(_db.ExecuteScalar("select count(id) from pedidos"));

            ViewBag.busqueda = q;

            ViewBag.totalPedidos = totalPedidos;
            ViewBag.totalPagina = Math.Ceiling((double)totalPedidos / ppp);
            ViewBag.pagina = pagina;

            return View(ListaPedidos);
        }


        public PartialViewResult BuscarArticulosModal()
        {
            return PartialView("/Views/Config/Modales/_Articulos.cshtml");
        }

        public PartialViewResult BuscarArticulosResultado(string q)
        {

            var articulos = _db.GetRecords<Articulo>(@"select A.*,  F.NombreArchivo from [Articulos] A
                                                        OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F  "
                                               + (!string.IsNullOrEmpty(q) ? "WHERE A.Nombre like '%" + q.Replace("'", "''") + "%'" : ""));

            var listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");

            ViewBag.listaCategoria = ordernarLista("", Guid.Empty, listaCategoria);



            return PartialView("/Views/Config/Parciales/_ResultadoArticulos.cshtml", articulos);
        }


        [HttpGet]
        public IActionResult PedidosEdit(string Id, string q, int pagina = 0)
        {
            const int ppp = 8;
            int offset = ppp * pagina;

            var ListaPedidos = _db.GetRecords<Pedidos>("SELECT * FROM [pedidos] WHERE [Id] = @id",
                new Dictionary<string, object>() {
                { "@id", Id}
            }).FirstOrDefault();

            ViewBag.listaClientes = _db.GetRecords<Usuarios>("SELECT * FROM [Usuarios]");
            ViewBag.listaCategoria = _db.GetRecords<Categoria>("SELECT * FROM [categorias]");
            ViewBag.listaArticulos = _db.GetRecords<Articulo>(@"select * from [Articulos]");
            ViewBag.listaPedidos_Articulos = _db.GetRecords<Pedidos_Articulos>(@"SELECT A.*,PA.* FROM [Articulos] A inner join  [Pedidos_Articulos] PA on A.Id= PA.IdArticulo inner join [Pedidos] P on PA.IdPedido=P.Id" +
             " where P.Id = @Id", new Dictionary<string, object>() { { "@Id", Id } });
            ViewBag.prueba = _db.GetRecords<Articulo>(@"SELECT * from [Articulos]");
            ViewBag.listaFotos = _db.GetRecords<Fotos>(@"select A.*,  F.* from [Articulos] A OUTER APPLY(SELECT TOP 1 * FROM[Fotos] F WHERE F.IdArticulo = A.Id) F");
            ViewBag.ListaCategorias = _db.GetRecords<Categoria>(@"select * from categorias");


            var totalArticulos = Convert.ToInt32(_db.ExecuteScalar("select count(id) from articulos where Estado='en venta'"));


            ViewBag.busqueda = q;


            ViewBag.totalArticulos = totalArticulos;
            ViewBag.totalPagina = Math.Ceiling((double)totalArticulos / ppp);
            ViewBag.pagina = pagina;



            if (ListaPedidos == null)
            {
                ListaPedidos = new Pedidos();
            }



            return View(ListaPedidos);



        }







        [HttpPost]
        public IActionResult PedidosEdit(string Id, string Estado, string FormaPago, string IdUsuario, decimal preciototal, string q, string articulos, double acumulador = 0)
        {
            //busqueda
            if (q != null /* || q == null*/ )
            {
                return PedidosEdit(Id, q);

            }

            List<string> idArticulos = new List<string>();
            if (!string.IsNullOrEmpty(articulos))
            {
                idArticulos = articulos.Split(',').ToList();
            }





            if (string.IsNullOrEmpty(Id) || Guid.Parse(Id) == Guid.Empty)
            {

                _db.ExecuteSQL("INSERT INTO [Pedidos]([Estado], [FormaPago],[IdUsuario],[Total],[CreadoPorMi]) values (@Estado, @FormaPago,@idusuario,@VentaTotal,'creado-por-mi')", new Dictionary<string, object>() {
                    { "@Estado",Estado },
                    { "@FormaPago",FormaPago },
                    { "@idusuario",IdUsuario},
                    {"@VentaTotal",preciototal}

                });

                Id = _db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Pedidos] ORDER BY [CreatedOn] DESC").ToString();



                //insertar pedidos_articulos de nuevo




                foreach (var idArticulo in idArticulos)
                {
                    //TODO completar la query
                    _db.ExecuteSQL("insert into [pedidos_articulos] (idpedido, idarticulo) values (@id, @articulo)", new Dictionary<string, object>() {
                    {"@id", Id },
                    {"@articulo", idArticulo }
                });
                }

                var idcreada = Convert.ToString(_db.ExecuteScalar("SELECT TOP 1 [Id] FROM [Pedidos] ORDER BY [CreatedOn] DESC"));
                Response.Redirect("/config/PedidosEdit/" + idcreada);

            }
            else
            {

                _db.ExecuteSQL("UPDATE [Pedidos] Set [Estado] =@estado, [FormaPago] = @formapago, [IdUsuario] = @idusuario,[Total] = @ventatotal where [Id]=@id", new Dictionary<string, object>() {
                    { "@Estado",Estado },
                    { "@FormaPago",FormaPago },
                    { "@idusuario",IdUsuario},
                    {"@VentaTotal",preciototal},
                    {"@id", Id }


                });


            }

            /*
            var precioarticulo = Convert.ToInt32(_db.ExecuteScalar("select precio from articulos where " +
                "id=(select top 1 idArticulo from Pedidos_Articulos where IdPedido =(select id from Pedidos where id=@id) and IdArticulo =(select id from articulos where id=@articulo))", new Dictionary<string, object>() {
                        {"@articulo",idArticulo },
                        {"@id", Id }}));
            acumulador = acumulador + precioarticulo;
            _db.ExecuteSQL("update Pedidos set total=" + acumulador + "where id=@id", new Dictionary<string, object>() {
                {"@id", Id } });
        }
            */

            return PedidosEdit(Id, q);
        }

        #endregion

        #region carousel

        [HttpGet]
        public IActionResult Carousel()
        {

            var carousel = _db.GetRecords<Carousel>(@"select * from carousel order by CreatedOn desc");

            return View(carousel);
        }

        [HttpPost]
        public IActionResult Carousel(string Id)
        {

            _db.ExecuteSQL("delete  FROM [carousel] where [id]= @Id", new Dictionary<string, object>() {
                { "@Id", Id}
            });

            return Carousel();

        }



        [HttpGet]
        public IActionResult CarouselEdit(string Id)
        {
            var carousel = _db.GetRecords<Carousel>("SELECT * FROM [Carousel] WHERE [Id] = @id",
                new Dictionary<string, object>() {
                { "@id", Id}
            }).FirstOrDefault();


            if (carousel == null)
            {
                carousel = new Carousel();
            }

            return View(carousel);
        }






        [HttpPost]
        public IActionResult CarouselEdit(string Id, string titulo, string descripcion)
        {

            foreach (var file in Request.Form.Files)
            {
                if (!System.IO.Directory.Exists(_env.WebRootPath + "/imagenes-carousel/"))
                {
                    System.IO.Directory.CreateDirectory(_env.WebRootPath + "/imagenes-carousel/");
                }

                using (var fileStream = new System.IO.FileStream(_env.WebRootPath + "/imagenes-carousel/" + file.FileName, System.IO.FileMode.Create))
                {
                    file.CopyTo(fileStream);
                    fileStream.Close();
                }

            }

            if (string.IsNullOrEmpty(Id) || Guid.Parse(Id) == Guid.Empty)
            {



                _db.ExecuteSQL("insert into [Carousel] (Titulo, Descripcion, Imagen) values (@titulo, @descripcion, @imagen)", new Dictionary<string, object>() {
                    {"@titulo", titulo },
                    {"@descripcion", descripcion },
                    {"@imagen", Request.Form.Files[0].FileName }

                });



                var idcreada = Convert.ToString(_db.ExecuteScalar("SELECT top 1 id FROM Carousel  ORDER BY [CreatedOn] DESC"));
                Response.Redirect("/config/carouseledit/" + idcreada);
            }
            else
            {



                _db.ExecuteSQL("update carousel set Titulo=@titulo,Descripcion=@descripcion,Imagen=@imagen where id=@Id", new Dictionary<string, object>() {
                    {"@titulo", titulo },
                    {"@descripcion", descripcion },
                    {"@Id",Id },
                    {"imagen", Request.Form.Files[0].FileName }
                });



            }

            return CarouselEdit(Id);

        }


        #endregion

        #region mantenimiento pagain
        #endregion


        #region admin

        public IActionResult Admin()
        {
            return View();
        }

        #endregion
    }
}