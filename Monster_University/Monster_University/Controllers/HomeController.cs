using System;
using System.Web.Mvc;

namespace Monster_University.Controllers
{
    
    public class HomeController : Controller
    {
        // GET: Home/Index - Dashboard principal
        public ActionResult Index()
        {
            // Obtener información del usuario para mostrar en el dashboard
            string nombreUsuario = Session["Usuario"]?.ToString() ?? User.Identity.Name;
            string usuarioId = Session["UsuarioID"]?.ToString() ?? "N/A";

            ViewBag.Usuario = nombreUsuario;
            ViewBag.UsuarioID = usuarioId;
            ViewBag.Titulo = "Panel de Control Principal";
            ViewBag.Fecha = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            return View();
        }

        // GET: Home/About - Acerca de
        public ActionResult About()
        {
            return View();
        }

        // GET: Home/Contact - Contacto
        public ActionResult Contact()
        {
            return View();
        }
    }
}