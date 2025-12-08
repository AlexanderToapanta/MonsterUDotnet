using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Monster_University.Controllers
{
    public class ControladorUsuarioController : Controller
    {
        // GET: ControladorUsuario/crearusuario
        public ActionResult crearusuario()
        {
            // Inicializar el modelo
            var model = new Usuario
            {
                XEUSU_ESTADO = "ACTIVO"
            };

            // Generar ID automáticamente
            var nuevoId = GenerarIdUsuarioAutomatico();
            ViewBag.IdGenerado = nuevoId;
            model.XEUSU_ID = nuevoId;

            return View(model);
        }

        // POST: ControladorUsuario/crearusuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult crearusuario(FormCollection form)
        {
            try
            {
                var nuevoUsuario = new Usuario
                {
                    XEUSU_ID = form["XEUSU_ID"],
                    PEPER_ID = string.IsNullOrEmpty(form["PEPER_ID"]) ? null : form["PEPER_ID"],
                    MECARR_ID = string.IsNullOrEmpty(form["MECARR_ID"]) ? null : form["MECARR_ID"],
                    MEEST_ID = string.IsNullOrEmpty(form["MEEST_ID"]) ? null : form["MEEST_ID"],
                    XEUSU_NOMBRE = form["XEUSU_NOMBRE"],
                    XEUSU_CONTRA = form["XEUSU_CONTRA"],
                    XEUSU_ESTADO = form["XEUSU_ESTADO"] ?? "ACTIVO"
                };

                // Si se especificó una persona, validar que no tenga usuario asignado
                if (!string.IsNullOrEmpty(nuevoUsuario.PEPER_ID))
                {
                    var personaConUsuario = CD_Personal.Instancia.ObtenerDetallePersonal(nuevoUsuario.PEPER_ID);
                    if (personaConUsuario != null && !string.IsNullOrEmpty(personaConUsuario.XEUSU_ID))
                    {
                        ViewBag.Error = "Esta persona ya tiene un usuario asignado.";
                        // Regenerar ID para nuevo intento
                        ViewBag.IdGenerado = GenerarIdUsuarioAutomatico();
                        return View(nuevoUsuario);
                    }
                }

                var respuesta = GuardarUsuario(nuevoUsuario);

                if (respuesta.estado)
                {
                    // Si se creó usuario para una persona, actualizar la persona
                    if (!string.IsNullOrEmpty(nuevoUsuario.PEPER_ID))
                    {
                        var persona = CD_Personal.Instancia.ObtenerDetallePersonal(nuevoUsuario.PEPER_ID);
                        if (persona != null)
                        {
                            persona.XEUSU_ID = nuevoUsuario.XEUSU_ID;
                            CD_Personal.Instancia.ModificarPersonal(persona);
                        }
                    }

                    TempData["SuccessMessage"] = respuesta.mensaje;
                    return RedirectToAction("crearusuario");
                }
                else
                {
                    ViewBag.Error = respuesta.mensaje;
                    ViewBag.IdGenerado = GenerarIdUsuarioAutomatico();
                    return View(nuevoUsuario);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.IdGenerado = GenerarIdUsuarioAutomatico();
                return View(new Usuario { XEUSU_ESTADO = "ACTIVO" });
            }
        }

        // GET: ControladorUsuario/listapersonas (para buscar personas)
        [HttpGet]
        public JsonResult BuscarPersonas(string term)
        {
            try
            {
                var personas = CD_Personal.Instancia.ObtenerPersonales();
                if (personas == null) return Json(new List<object>(), JsonRequestBehavior.AllowGet);

                var resultados = new List<object>();
                foreach (var persona in personas)
                {
                    // Solo mostrar personas que no tengan usuario asignado
                    if (string.IsNullOrEmpty(persona.XEUSU_ID))
                    {
                        resultados.Add(new
                        {
                            id = persona.PEPER_ID,
                            text = $"{persona.PEPER_ID} - {persona.PEPER_NOMBRE} {persona.PEPER_APELLIDO}",
                            nombre = persona.PEPER_NOMBRE,
                            apellido = persona.PEPER_APELLIDO,
                            cedula = persona.PEPER_CEDULA
                        });
                    }
                }
                return Json(resultados, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ControladorUsuario/generardatosusuario
        [HttpPost]
        public JsonResult GenerarDatosUsuario(string personaId)
        {
            try
            {
                if (string.IsNullOrEmpty(personaId))
                    return Json(new { success = false, message = "ID de persona requerido" });

                var persona = CD_Personal.Instancia.ObtenerDetallePersonal(personaId);
                if (persona == null)
                    return Json(new { success = false, message = "Persona no encontrada" });

                // Verificar si ya tiene usuario
                if (!string.IsNullOrEmpty(persona.XEUSU_ID))
                    return Json(new { success = false, message = "Esta persona ya tiene un usuario asignado" });

                // Generar datos automáticos
                var datosUsuario = new
                {
                    success = true,
                    nombreUsuario = GenerarNombreUsuario(persona),
                    contrasena = persona.PEPER_CEDULA
                };

                return Json(datosUsuario);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
            try
            {
                var usuarioEditado = new Usuario
                {
                    XEUSU_ID = form["XEUSU_ID"],
                    PEPER_ID = string.IsNullOrEmpty(form["PEPER_ID"]) ? null : form["PEPER_ID"],
                    MECARR_ID = string.IsNullOrEmpty(form["MECARR_ID"]) ? null : form["MECARR_ID"],
                    MEEST_ID = string.IsNullOrEmpty(form["MEEST_ID"]) ? null : form["MEEST_ID"],
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
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Lista");
            }
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

        private string GenerarIdUsuarioAutomatico()
        {
            try
            {
                // Obtener lista de usuarios existentes
                var listaUsuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                if (listaUsuarios == null || listaUsuarios.Count == 0)
                {
                    return "US001";
                }

                // Buscar máximo número en IDs USXXX
                int maxNumero = 0;
                foreach (var usuario in listaUsuarios)
                {
                    if (usuario.XEUSU_ID != null &&
                        usuario.XEUSU_ID.StartsWith("US") &&
                        usuario.XEUSU_ID.Length == 5)
                    {
                        try
                        {
                            string numeroStr = usuario.XEUSU_ID.Substring(2);
                            if (int.TryParse(numeroStr, out int numero))
                            {
                                if (numero > maxNumero)
                                {
                                    maxNumero = numero;
                                }
                            }
                        }
                        catch { }
                    }
                }

                // Buscar huecos disponibles
                for (int i = 1; i <= 999; i++)
                {
                    string idCandidato = $"US{i:000}";
                    bool existe = listaUsuarios.Any(u => u.XEUSU_ID == idCandidato);
                    if (!existe)
                    {
                        return idCandidato;
                    }
                }

                // Si no hay huecos, usar siguiente número
                return $"US{maxNumero + 1:000}";
            }
            catch (Exception)
            {
                return "US001";
            }
        }

        private string GenerarNombreUsuario(Personal persona)
        {
            if (string.IsNullOrEmpty(persona.PEPER_NOMBRE) || string.IsNullOrEmpty(persona.PEPER_APELLIDO))
            {
                return "usuario_" + persona.PEPER_CEDULA;
            }

            string primeraLetra = persona.PEPER_NOMBRE.Substring(0, 1).ToUpper();
            string nombreUsuario = primeraLetra + persona.PEPER_APELLIDO;

            // Limitar longitud si es necesario
            if (nombreUsuario.Length > 100)
            {
                nombreUsuario = nombreUsuario.Substring(0, 100);
            }

            return nombreUsuario;
        }

        public Respuesta<List<Usuario>> ObtenerUsuarios()
        {
            Respuesta<List<Usuario>> response = new Respuesta<List<Usuario>>();
            try
            {
                List<Usuario> lista = CD_Usuario.Instancia.ObtenerUsuarios();

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
                Usuario usuario = CD_Usuario.Instancia.ObtenerDetalleUsuario(XEUSU_ID);

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
                List<Usuario> lista = CD_Usuario.Instancia.ObtenerUsuarios();
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

                bool resultado = CD_Usuario.Instancia.RegistrarUsuario(oUsuario);

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

                bool resultado = CD_Usuario.Instancia.ModificarUsuario(oUsuario);

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

                bool resultado = CD_Usuario.Instancia.EliminarUsuario(XEUSU_ID);

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
