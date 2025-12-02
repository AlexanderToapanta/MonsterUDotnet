using System;
using System.Web.Mvc;
using System.Web.Security;
using CapaDatos;


namespace Monster_University.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login/Login
        public ActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            var respuesta = LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

            if (respuesta.estado)
            {
                FormsAuthentication.SetAuthCookie(XEUSU_NOMBRE, false);
                Session["Usuario"] = XEUSU_NOMBRE;

                // Obtener el ID del usuario para guardarlo en sesión
                var usuarioDetalle = ObtenerUsuarioPorNombre(XEUSU_NOMBRE);
                if (usuarioDetalle.estado && usuarioDetalle.objeto != null)
                {
                    Session["UsuarioID"] = usuarioDetalle.objeto.XEUSU_ID;
                }

                return RedirectToAction("Index", "Home"); // Redirige al Home/Index
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View();
            }
        }

        // GET: Login/Index (para el logout u otras funciones)
        public ActionResult Index()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            // Si no está autenticado, mostrar el login
            return RedirectToAction("Login");
        }

        // GET: Login/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Métodos auxiliares
        private Respuesta<int> LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            Respuesta<int> response = new Respuesta<int>();
            try
            {
                if (string.IsNullOrEmpty(XEUSU_NOMBRE))
                {
                    response.estado = false;
                    response.mensaje = "El nombre de usuario es requerido";
                    return response;
                }

                if (string.IsNullOrEmpty(XEUSU_CONTRA))
                {
                    response.estado = false;
                    response.mensaje = "La contraseña es requerida";
                    return response;
                }

                int resultado = CD_Usuario.Instancia.LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

                response.estado = resultado > 0;
                response.objeto = resultado;
                response.mensaje = resultado > 0 ? "Login exitoso" : "Usuario o contraseña incorrectos";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        private Respuesta<Usuario> ObtenerUsuarioPorNombre(string XEUSU_NOMBRE)
        {
            Respuesta<Usuario> response = new Respuesta<Usuario>();
            try
            {
                // Necesitarás implementar este método en CD_Usuario
                // Por ahora, usaremos un método temporal
                var lista = CD_Usuario.Instancia.ObtenerUsuarios();
                Usuario usuario = lista?.Find(u => u.XEUSU_NOMBRE == XEUSU_NOMBRE);

                response.estado = usuario != null;
                response.objeto = usuario;
                response.mensaje = usuario != null ? "Usuario obtenido correctamente" : "Usuario no encontrado";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        private class Respuesta<T>
        {
            public bool estado { get; set; }
            public string mensaje { get; set; }
            public T objeto { get; set; }
        }
    }
}