using System.Web.Mvc;

namespace Monster_University.Controllers
{
    public class ViewsController : Controller
    {
        // GET: /Views/Index - Dashboard principal
        [Authorize]
        public ActionResult Index()
        {
            
            return View("Index");
        }

        // GET: /Views/Dashboard - Alternativo
        [Authorize]
        public ActionResult Dashboard()
        {
            ViewBag.Usuario = Session["Usuario"] ?? User.Identity.Name;
            ViewBag.Titulo = "Panel de Control";
            return View();
        }

        // GET: /Views/About - Acerca de
        public ActionResult About()
        {
            return View();
        }

        // GET: /Views/Contact - Contacto
        public ActionResult Contact()
        {
            return View();
        }
    }
}