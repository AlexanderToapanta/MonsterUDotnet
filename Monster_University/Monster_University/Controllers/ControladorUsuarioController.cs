using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;

namespace Monster_University.Controllers
{
    public class ControladorUsuarioController : Controller
    {
       
        public ActionResult crearusuario()
        {
            // Inicializar el modelo con estado predeterminado
            var model = new Usuario
            {
                XEUSU_ESTADO = "ACTIVO"
            };
            return View(model);
        }

        // POST: ControladorUsuario/crearusuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult crearusuario(FormCollection form)
        {
            var nuevoUsuario = new Usuario
            {
                XEUSU_ID = form["UsuarioID"],
                PEPER_ID = form["PEPER_ID"],
                MECARR_ID = form["MECARR_ID"],
                MEEST_ID = form["MEEST_ID"],
                XEUSU_NOMBRE = form["NombreUsuario"],
                XEUSU_CONTRA = form["Contrasena"],
                XEUSU_ESTADO = form["Estado"] ?? "ACTIVO"
            };

            // Validar confirmación de contraseña si existe
            string confirmarContrasena = form["ConfirmarContrasena"];
            if (!string.IsNullOrEmpty(confirmarContrasena) && nuevoUsuario.XEUSU_CONTRA != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View(nuevoUsuario);
            }

            var respuesta = GuardarUsuario(nuevoUsuario);

            if (respuesta.estado)
            {
                TempData["SuccessMessage"] = respuesta.mensaje;
                return RedirectToAction("crearusuario");
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View(nuevoUsuario);
            }
        }

        // GET: ControladorUsuario/CambiarContrasena
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
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = respuesta.mensaje;
                return View();
            }
        }


        // GET: ControladorUsuario/Lista
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

        // GET: ControladorUsuario/Editar/{id}
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

        // POST: ControladorUsuario/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(FormCollection form)
        {
            var usuarioEditado = new Usuario
            {
                XEUSU_ID = form["XEUSU_ID"],
                PEPER_ID = form["PEPER_ID"],
                MECARR_ID = form["MECARR_ID"],
                MEEST_ID = form["MEEST_ID"],
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

        // GET: ControladorUsuario/Eliminar/{id}
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

        // POST: ControladorUsuario/Eliminar/{id}
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

        // GET: ControladorUsuario/Detalles/{id}
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


       
        // Métodos auxiliares de negocio

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

        public Respuesta<Usuario> ObtenerUsuarioPorNombre(string XEUSU_NOMBRE)
        {
            Respuesta<Usuario> response = new Respuesta<Usuario>();
            try
            {
                // Como no tenemos un método específico, obtenemos todos y filtramos
                List<Usuario> lista = CapaDatos.CD_Usuario.Instancia.ObtenerUsuarios();
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

        public Respuesta<bool> GuardarUsuario(Usuario oUsuario)
        {
            Respuesta<bool> response = new Respuesta<bool>();
            try
            {
                // Validaciones
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

                // Validar longitud mínima de contraseña
                if (oUsuario.XEUSU_CONTRA.Length < 6)
                {
                    response.estado = false;
                    response.mensaje = "La contraseña debe tener al menos 6 caracteres";
                    return response;
                }

                // Validar que si se especifica MEEST_ID, también se especifique MECARR_ID
                if (!string.IsNullOrEmpty(oUsuario.MEEST_ID) && string.IsNullOrEmpty(oUsuario.MECARR_ID))
                {
                    response.estado = false;
                    response.mensaje = "Si especifica Estudiante ID, debe especificar Carrera ID";
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

                if (string.IsNullOrEmpty(oUsuario.XEUSU_NOMBRE))
                {
                    response.estado = false;
                    response.mensaje = "El nombre de usuario es requerido";
                    return response;
                }

                // Validar que si se especifica MEEST_ID, también se especifique MECARR_ID
                if (!string.IsNullOrEmpty(oUsuario.MEEST_ID) && string.IsNullOrEmpty(oUsuario.MECARR_ID))
                {
                    response.estado = false;
                    response.mensaje = "Si especifica Estudiante ID, debe especificar Carrera ID";
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

                // Prevenir eliminación del usuario actualmente logueado
                string usuarioActualId = Session["UsuarioID"]?.ToString();
                if (usuarioActualId == XEUSU_ID)
                {
                    response.estado = false;
                    response.mensaje = "No puede eliminar su propio usuario mientras está logueado";
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

                if (string.IsNullOrEmpty(XEUSU_CONTRA))
                {
                    response.estado = false;
                    response.mensaje = "La contraseña actual es requerida";
                    return response;
                }

                if (string.IsNullOrEmpty(nuevaClave))
                {
                    response.estado = false;
                    response.mensaje = "La nueva contraseña es requerida";
                    return response;
                }

                // Validar longitud mínima de nueva contraseña
                if (nuevaClave.Length < 6)
                {
                    response.estado = false;
                    response.mensaje = "La nueva contraseña debe tener al menos 6 caracteres";
                    return response;
                }

                int resultado = CapaDatos.CD_Usuario.Instancia.CambiarClave(XEUSU_NOMBRE, XEUSU_CONTRA, nuevaClave);

                response.estado = resultado > 0;
                response.objeto = resultado;
                response.mensaje = resultado > 0 ? "Contraseña cambiada correctamente" : "No se pudo cambiar la contraseña. Verifique su contraseña actual.";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }
    }

    // Clase auxiliar para manejar respuestas
    public class Respuesta<T>
    {
        public bool estado { get; set; }
        public string mensaje { get; set; }
        public T objeto { get; set; }
    }
}