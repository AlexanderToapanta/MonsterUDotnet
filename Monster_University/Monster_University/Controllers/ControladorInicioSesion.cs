using System;
using System.Web.Mvc;
using CapaDatos;
using System.Web.Security;

namespace Monster_University.Controllers
{
    public class ControladorInicioSesionController : Controller
    {
       
        public ActionResult Login()
        {
            
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Usuario, string Clave)
        {
            try
            {
                if (string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(Clave))
                {
                    ViewBag.Error = "Usuario y contraseña son requeridos";
                    return View();
                }

                int resultado = CD_Usuario.Instancia.LoginUsuario(Usuario, Clave);

                if (resultado > 0)
                {
                    
                    FormsAuthentication.SetAuthCookie(Usuario, false);

                   
                    return RedirectToAction("Index", "Views");
                }
                else
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en el servidor: " + ex.Message;
                return View();
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "ControladorInicioSesion");
        }
    }
}