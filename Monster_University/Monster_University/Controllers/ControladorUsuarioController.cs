using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using CapaModelo;

namespace Monster_University.Controllers
{
    public class ControladorUsuarioController : Controller
    {
     
        public ActionResult Login()
        {
            return View();
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            var respuesta = LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

            if (respuesta.estado)
            {
                FormsAuthentication.SetAuthCookie(XEUSU_NOMBRE, false);
                Session["Usuario"] = XEUSU_NOMBRE;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View();
            }
        }

       
        public ActionResult crearusuario()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult crearusuario(FormCollection form)
        {
            var nuevoUsuario = new Usuario
            {
                XEUSU_ID = form["UsuarioID"],
                XEUSU_NOMBRE = form["NombreUsuario"],
                XEUSU_CONTRA = form["Contrasena"],
                XEUSU_ESTADO = form["Estado"] ?? "ACTIVO"
            };

            var respuesta = GuardarUsuario(nuevoUsuario);

            if (respuesta.estado)
            {
                TempData["SuccessMessage"] = respuesta.mensaje;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View();
            }
        }

        
        public ActionResult CambiarContrasena()
        {
            return View("cambiarcontrasena");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena(string claveActual, string nuevaClave, string confirmarClave)
        {
            if (nuevaClave != confirmarClave)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View();
            }

            string usuarioActual = Session["Usuario"]?.ToString() ?? User.Identity.Name;
            var respuesta = CambiarClaveUsuario(usuarioActual, claveActual, nuevaClave);

            if (respuesta.estado)
            {
                TempData["SuccessMessage"] = respuesta.mensaje;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View();
            }
        }

  
        public ActionResult Lista()
        {
            var respuesta = ObtenerUsuarios();

            if (respuesta.estado)
            {
                return View(respuesta.objeto);
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View(new List<Usuario>());
            }
        }

        
        public ActionResult Editar(string id)
        {
            var respuesta = ObtenerUsuarioPorId(id);

            if (respuesta.estado)
            {
                return View(respuesta.objeto);
            }
            else
            {
                TempData["ErrorMessage"] = respuesta.mensaje;
                return RedirectToAction("Lista");
            }
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(FormCollection form)
        {
            var usuarioEditado = new Usuario
            {
                XEUSU_ID = form["XEUSU_ID"],
                XEUSU_NOMBRE = form["XEUSU_NOMBRE"],
                XEUSU_CONTRA = form["XEUSU_CONTRA"],
                XEUSU_ESTADO = form["XEUSU_ESTADO"]
            };

            var respuesta = EditarUsuario(usuarioEditado);

            if (respuesta.estado)
            {
                TempData["SuccessMessage"] = respuesta.mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = respuesta.mensaje;
            }

            return RedirectToAction("Lista");
        }

        public ActionResult Eliminar(string id)
        {
            var respuesta = ObtenerUsuarioPorId(id);

            if (respuesta.estado)
            {
                return View(respuesta.objeto);
            }
            else
            {
                TempData["ErrorMessage"] = respuesta.mensaje;
                return RedirectToAction("Lista");
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Eliminar")]
        public ActionResult EliminarConfirmado(string id)
        {
            var respuesta = EliminarUsuario(id);

            if (respuesta.estado)
            {
                TempData["SuccessMessage"] = respuesta.mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = respuesta.mensaje;
            }

            return RedirectToAction("Lista");
        }

        
        public ActionResult Detalles(string id)
        {
            var respuesta = ObtenerUsuarioPorId(id);

            if (respuesta.estado)
            {
                return View(respuesta.objeto);
            }
            else
            {
                TempData["ErrorMessage"] = respuesta.mensaje;
                return RedirectToAction("Lista");
            }
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "ControladorUsuario");
        }

    
        public Respuesta<List<Usuario>> ObtenerUsuarios()
        {
            Respuesta<List<Usuario>> response = new Respuesta<List<Usuario>>();
            try
            {
                List<Usuario> lista = CapaDatos.CD_Usuario.Instancia.ObtenerUsuarios();

                response.estado = lista != null;
                response.objeto = lista;
                response.mensaje = lista != null ? "Datos obtenidos correctamente" : "No se encontraron datos";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        public Respuesta<Usuario> ObtenerUsuarioPorId(string XEUSU_ID)
        {
            Respuesta<Usuario> response = new Respuesta<Usuario>();
            try
            {
                Usuario usuario = CapaDatos.CD_Usuario.Instancia.ObtenerDetalleUsuario(XEUSU_ID);

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

        public Respuesta<bool> GuardarUsuario(Usuario oUsuario)
        {
            Respuesta<bool> response = new Respuesta<bool>();
            try
            {
                if (string.IsNullOrEmpty(oUsuario.XEUSU_ID))
                {
                    response.estado = false;
                    response.mensaje = "El ID del usuario es requerido";
                    return response;
                }

                if (string.IsNullOrEmpty(oUsuario.XEUSU_NOMBRE))
                {
                    response.estado = false;
                    response.mensaje = "El nombre de usuario es requerido";
                    return response;
                }

                if (string.IsNullOrEmpty(oUsuario.XEUSU_CONTRA))
                {
                    response.estado = false;
                    response.mensaje = "La contraseña es requerida";
                    return response;
                }

                if (string.IsNullOrEmpty(oUsuario.XEUSU_ESTADO))
                {
                    oUsuario.XEUSU_ESTADO = "ACTIVO";
                }

                bool resultado = CapaDatos.CD_Usuario.Instancia.RegistrarUsuario(oUsuario);

                response.estado = resultado;
                response.objeto = resultado;
                response.mensaje = resultado ? "Usuario registrado correctamente" : "No se pudo registrar el usuario";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        public Respuesta<bool> EditarUsuario(Usuario oUsuario)
        {
            Respuesta<bool> response = new Respuesta<bool>();
            try
            {
                if (string.IsNullOrEmpty(oUsuario.XEUSU_ID))
                {
                    response.estado = false;
                    response.mensaje = "El ID del usuario es requerido";
                    return response;
                }

                bool resultado = CapaDatos.CD_Usuario.Instancia.ModificarUsuario(oUsuario);

                response.estado = resultado;
                response.objeto = resultado;
                response.mensaje = resultado ? "Usuario actualizado correctamente" : "No se pudo actualizar el usuario";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        public Respuesta<bool> EliminarUsuario(string XEUSU_ID)
        {
            Respuesta<bool> response = new Respuesta<bool>();
            try
            {
                if (string.IsNullOrEmpty(XEUSU_ID))
                {
                    response.estado = false;
                    response.mensaje = "El ID del usuario es requerido";
                    return response;
                }

                bool resultado = CapaDatos.CD_Usuario.Instancia.EliminarUsuario(XEUSU_ID);

                response.estado = resultado;
                response.objeto = resultado;
                response.mensaje = resultado ? "Usuario eliminado correctamente" : "No se pudo eliminar el usuario";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        public Respuesta<int> LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
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

                int resultado = CapaDatos.CD_Usuario.Instancia.LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

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

        public Respuesta<int> CambiarClaveUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA, string nuevaClave)
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

                if (string.IsNullOrEmpty(nuevaClave))
                {
                    response.estado = false;
                    response.mensaje = "La nueva contraseña es requerida";
                    return response;
                }

                int resultado = CapaDatos.CD_Usuario.Instancia.CambiarClave(XEUSU_NOMBRE, XEUSU_CONTRA, nuevaClave);

                response.estado = resultado > 0;
                response.objeto = resultado;
                response.mensaje = resultado > 0 ? "Contraseña cambiada correctamente" : "No se pudo cambiar la contraseña";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }
    }

    public class Respuesta<T>
    {
        public bool estado { get; set; }
        public string mensaje { get; set; }
        public T objeto { get; set; }
    }
}