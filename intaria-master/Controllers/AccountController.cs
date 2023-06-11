using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Intaria.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using qDatabase;

namespace CookieDemo.Controllers
{

    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        
        public AccountController(IConfiguration config)
        {
            this._config = config;
        }

       

        /*
        private SignInManager<Usuarios> signInManager;
        private UserManager<Usuarios> userManager;
        */

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(string nombre, string clave, string inputlogin, string inputreg, string email, string clave1, string clave2, string Nombregoogle, string Emailgoogle, string logingoogle, string NombreFacebook, string EmailFacebook, string loginFacebook)
        {

           Database _db = new Database(_config.GetConnectionString("DefaultConnection"));

            /* Inicio de sesión en google*/
            if (logingoogle == "si" )
            {

                ClaimsIdentity identity = null;
                bool isAuthenticated = false;
                var existeemail = _db.ExecuteScalar("SELECT UserId FROM [usuarios] WHERE [Email] = @email", new Dictionary<string, object>() { { "@email", Emailgoogle } });


                if (existeemail == null)
                {

                    var existenombre = _db.ExecuteScalar("SELECT UserId FROM [usuarios] WHERE [Nombre] = @nombre", new Dictionary<string, object>() { { "@nombre", Nombregoogle } });

                    if (existenombre != null)
                    {
                        var rand = new Random();

                        // Generate and display 5 random byte (integer) values.
                        var bytes = new byte[5];

                        rand.NextBytes(bytes);

                        foreach (byte byteValue in bytes)
                        {

                            Nombregoogle += byteValue;
                        }
                    }

                    _db.ExecuteSQL("INSERT INTO [Usuarios]([Nombre],[Email],[CreadoPorMi]) values (@nombre,@email,'no')", new Dictionary<string, object>() {
                    { "@nombre",Nombregoogle },
                    { "@email",Emailgoogle },
                });

                }
                else
                {
                    var nombreusuario = Convert.ToString(_db.ExecuteScalar("SELECT Nombre FROM [usuarios] WHERE [email] = @emailgoogle or  [email] = @emailfacebook", new Dictionary<string, object>() { { "@emailgoogle", Emailgoogle }, { "@emailfacebook", EmailFacebook } }));
                    Nombregoogle = nombreusuario;

                }


                //Create the identity for the user
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, Nombregoogle),
                    new Claim(ClaimTypes.Role, "User")
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                isAuthenticated = true;


                if (isAuthenticated)
                {
                    var principal = new ClaimsPrincipal(identity);

                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("index", "Web");
                }
            }




            /* Inicio de sesion */
            if (inputlogin == "forminputlog")
            {
                if (!string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(clave))
                {
                    return RedirectToAction("Login", "Account");
                }

                ClaimsIdentity identity = null;
                bool isAuthenticated = false;



                var existe = _db.ExecuteScalar("SELECT  UserId FROM [Usuarios] where clave=@Clave and  Email=@Nombre ",
                 new Dictionary<string, object>() {
                { "@Nombre", nombre},
                 {"@Clave", clave } });

                if (existe != null)
                {
                    //Create the identity for the user
                    identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, nombre),
                    new Claim(ClaimTypes.Role, "User")
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                    isAuthenticated = true;
                }
                else
                {
                    ViewBag.mensajeerror = "The data entered is not correct";
                }

                if (isAuthenticated)
                {
                    var principal = new ClaimsPrincipal(identity);

                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("index", "Web");
                }
            }

            /*  Registro */

            if (inputreg == "forminputreg")
            {
                var existemail = _db.ExecuteScalar("SELECT UserId FROM [usuarios] WHERE [Email] = @email", new Dictionary<string, object>() { { "@email", email } });


                if (existemail != null)
                {
                    ViewBag.ErrorRegistro = "this email already exists";

                }

                else if (clave1 == null || clave2 == null)
                {
                    ViewBag.ErrorRegistro = "You haven´t written any password";

                }

                else if (clave1.Length <= 6)
                {
                    ViewBag.ErrorRegistro = "the password must be at least 6 characters long";
                }

                else if (clave1 == clave2)
                {

                    _db.ExecuteSQL("INSERT INTO [usuarios] ([Nombre],[Email],[Clave],[CreadoPorMi]) values (@email,@email, @clave,'no')",
                     new Dictionary<string, object>()
                     {
                        {"@email", email },
                        {"@clave",clave1},
                   });
                    ViewBag.UsuarioRegistrado = "The user has been successfully registered";
                }

                else
                {
                    ViewBag.ErrorRegistro = "Passwords do not match";
                }


            }

            return View();
        }


        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        public IActionResult Admin()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Admin(string nombredmin, string claveadmin)
        {
            if (!string.IsNullOrEmpty(nombredmin) && string.IsNullOrEmpty(claveadmin))
            {
                return RedirectToAction("Login");
            }

            //Check the user name and password  
            //Here can be implemented checking logic from the database  
            ClaimsIdentity identity = null;
            bool isAuthenticated = false;

            if (nombredmin == "xxxxxx" && claveadmin == "xxxxxx")
            {

                //Create the identity for the user  
                identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, nombredmin),
                    new Claim(ClaimTypes.Role, "Admin")
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                isAuthenticated = true;
            }



            if (isAuthenticated)
            {
                var principal = new ClaimsPrincipal(identity);

                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Config");
            }

            return View();
        }
    }
}

