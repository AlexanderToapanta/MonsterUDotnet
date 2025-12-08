using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaModelo;
using CapaDatos;

namespace Monster_University.Controllers
{
    public class ControladorPersonalController : Controller
    {
        // GET: ControladorPersonal/crearpersonal
        public ActionResult crearpersonal()
        {
            // Inicializar nueva persona
            var model = new Personal();
            model.PEPEPER_FECH_INGR = DateTime.Now; // Fecha actual por defecto

            // Cargar datos necesarios para la vista
            ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
            ViewBag.TiposPersonal = CD_Personal.Instancia.ObtenerTiposPersonal();

            // Generar ID automáticamente
            var nuevoId = GenerarIdPersonaAutomatico();
            ViewBag.IdGenerado = nuevoId;
            model.PEPER_ID = nuevoId;

            return View(model);
        }

        // POST: ControladorPersonal/crearpersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult crearpersonal(FormCollection form)
        {
            try
            {
                // Crear objeto persona
                var nuevaPersona = new Personal
                {
                    PEPER_ID = form["PEPER_ID"],
                    PEPER_NOMBRE = form["PEPER_NOMBRE"],
                    PEPER_APELLIDO = form["PEPER_APELLIDO"],
                    PEPER_EMAIL = form["PEPER_EMAIL"],
                    PEPER_CEDULA = form["PEPER_CEDULA"],
                    PEPER_CELULAR = form["PEPER_CELULAR"],
                    PEPER_TIPO = form["PEPER_TIPO"],
                    PESEX_ID = form["PESEX_ID"], // Obligatorio
                    PEESC_ID = string.IsNullOrEmpty(form["PEESC_ID"]) ? null : form["PEESC_ID"], // Opcional
                    XEUSU_ID = null // Se establecerá después si se crea usuario
                };

                // Parsear fecha de ingreso
                if (DateTime.TryParse(form["PEPEPER_FECH_INGR"], out DateTime fechaIngreso))
                {
                    nuevaPersona.PEPEPER_FECH_INGR = fechaIngreso;
                }
                else
                {
                    nuevaPersona.PEPEPER_FECH_INGR = DateTime.Now;
                }

                // Validaciones
                if (!ValidarDatosPersona(nuevaPersona))
                {
                    ViewBag.Error = "Datos inválidos. Revise los campos requeridos.";
                    // Recargar datos para la vista
                    ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                    ViewBag.TiposPersonal = CD_Personal.Instancia.ObtenerTiposPersonal();
                    return View(nuevaPersona);
                }

                // PASO 1: Guardar persona
                bool personaCreada = CD_Personal.Instancia.RegistrarPersonal(nuevaPersona);

                if (!personaCreada)
                {
                    ViewBag.Error = "Error al guardar la persona.";
                    // Recargar datos para la vista
                    ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                    ViewBag.TiposPersonal = CD_Personal.Instancia.ObtenerTiposPersonal();
                    return View(nuevaPersona);
                }

                // PASO 2: Crear usuario automáticamente
                var usuarioCreado = CrearUsuarioParaPersona(nuevaPersona);

                string mensaje;
                if (usuarioCreado != null)
                {
                    // PASO 3: Actualizar persona con ID de usuario
                    nuevaPersona.XEUSU_ID = usuarioCreado.XEUSU_ID;
                    CD_Personal.Instancia.ModificarPersonal(nuevaPersona);

                    mensaje = $"Persona creada con ID: {nuevaPersona.PEPER_ID} y Usuario creado con ID: {usuarioCreado.XEUSU_ID}";
                    TempData["SuccessMessage"] = mensaje;
                }
                else
                {
                    mensaje = $"Persona creada con ID: {nuevaPersona.PEPER_ID} pero no se pudo crear el usuario automático.";
                    TempData["WarningMessage"] = mensaje;
                }

                // Redireccionar para limpiar formulario
                return RedirectToAction("crearpersonal");

            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al crear persona: {ex.Message}";
                // Recargar datos para la vista
                ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                ViewBag.TiposPersonal = CD_Personal.Instancia.ObtenerTiposPersonal();

                return View(new Personal());
            }
        }

        // GET: ControladorPersonal/listapersonal
        public ActionResult listapersonal()
        {
            var listaPersonal = CD_Personal.Instancia.ObtenerPersonales();
            if (listaPersonal == null)
            {
                ViewBag.Error = "Error al cargar la lista de personal.";
                return View(new List<Personal>());
            }
            return View(listaPersonal);
        }

        // GET: ControladorPersonal/editarpersonal/{id}
        public ActionResult editarpersonal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerDetallePersonal(id);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            // Cargar datos necesarios para la vista
            ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
            ViewBag.TiposPersonal = CD_Personal.Instancia.ObtenerTiposPersonal();

            return View(personal);
        }

        // POST: ControladorPersonal/editarpersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editarpersonal(FormCollection form)
        {
            try
            {
                var personalEditado = new Personal
                {
                    PEPER_ID = form["PEPER_ID"],
                    PEPER_NOMBRE = form["PEPER_NOMBRE"],
                    PEPER_APELLIDO = form["PEPER_APELLIDO"],
                    PEPER_EMAIL = form["PEPER_EMAIL"],
                    PEPER_CEDULA = form["PEPER_CEDULA"],
                    PEPER_CELULAR = form["PEPER_CELULAR"],
                    PEPER_TIPO = form["PEPER_TIPO"],
                    PESEX_ID = form["PESEX_ID"],
                    PEESC_ID = string.IsNullOrEmpty(form["PEESC_ID"]) ? null : form["PEESC_ID"],
                    XEUSU_ID = string.IsNullOrEmpty(form["XEUSU_ID"]) ? null : form["XEUSU_ID"]
                };

                // Parsear fecha
                if (DateTime.TryParse(form["PEPEPER_FECH_INGR"], out DateTime fechaIngreso))
                {
                    personalEditado.PEPEPER_FECH_INGR = fechaIngreso;
                }

                // Validaciones
                if (!ValidarDatosPersona(personalEditado, true))
                {
                    TempData["ErrorMessage"] = "Datos inválidos.";
                    return RedirectToAction("editarpersonal", new { id = personalEditado.PEPER_ID });
                }

                bool resultado = CD_Personal.Instancia.ModificarPersonal(personalEditado);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Personal actualizado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el personal.";
                }

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        // GET: ControladorPersonal/eliminarpersonal/{id}
        public ActionResult eliminarpersonal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerDetallePersonal(id);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            return View(personal);
        }

        // POST: ControladorPersonal/eliminarpersonal/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("eliminarpersonal")]
        public ActionResult eliminarpersonalconfirmado(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "ID de personal requerido.";
                    return RedirectToAction("listapersonal");
                }

                bool resultado = CD_Personal.Instancia.EliminarPersonal(id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Personal eliminado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el personal. Verifique que no tenga usuarios o grupos relacionados.";
                }

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        // GET: ControladorPersonal/detallespersonal/{id}
        public ActionResult detallespersonal(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerDetallePersonal(id);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            return View(personal);
        }

        // Métodos auxiliares (similares a los de Java)

        private string GenerarIdPersonaAutomatico()
        {
            try
            {
                // Obtener lista de personal existente
                var listaPersonal = CD_Personal.Instancia.ObtenerPersonales();
                if (listaPersonal == null || listaPersonal.Count == 0)
                {
                    return "PE001";
                }

                // Buscar máximo número en IDs PEXXX
                int maxNumero = 0;
                foreach (var persona in listaPersonal)
                {
                    if (persona.PEPER_ID != null &&
                        persona.PEPER_ID.StartsWith("PE") &&
                        persona.PEPER_ID.Length == 5)
                    {
                        try
                        {
                            string numeroStr = persona.PEPER_ID.Substring(2);
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
                    string idCandidato = $"PE{i:000}";
                    bool existe = listaPersonal.Any(p => p.PEPER_ID == idCandidato);
                    if (!existe)
                    {
                        return idCandidato;
                    }
                }

                // Si no hay huecos, usar siguiente número
                return $"PE{maxNumero + 1:000}";
            }
            catch (Exception)
            {
                return "PE001";
            }
        }

        private Usuario CrearUsuarioParaPersona(Personal persona)
        {
            try
            {
               
                string usuarioId = GenerarIdUsuario();

                
                string nombreUsuario = GenerarNombreUsuario(persona);

                
                string contrasenia = persona.PEPER_CEDULA;

               
                var nuevoUsuario = new Usuario
                {
                    XEUSU_ID = usuarioId,
                    XEUSU_NOMBRE = nombreUsuario,
                    XEUSU_CONTRA = contrasenia,
                    XEUSU_ESTADO = "ACTIVO",
                    PEPER_ID = persona.PEPER_ID 
                };

                
                bool usuarioCreado = CD_Usuario.Instancia.RegistrarUsuario(nuevoUsuario);

                return usuarioCreado ? nuevoUsuario : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GenerarIdUsuario()
        {
            try
            {
               
                var listaUsuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                if (listaUsuarios == null || listaUsuarios.Count == 0)
                {
                    return "US001";
                }

                
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

               
                for (int i = 1; i <= 999; i++)
                {
                    string idCandidato = $"US{i:000}";
                    bool existe = listaUsuarios.Any(u => u.XEUSU_ID == idCandidato);
                    if (!existe)
                    {
                        return idCandidato;
                    }
                }

               
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

        private bool ValidarDatosPersona(Personal persona, bool esEdicion = false)
        {
            // Validaciones básicas
            if (string.IsNullOrEmpty(persona.PEPER_ID))
                return false;

            if (string.IsNullOrEmpty(persona.PEPER_NOMBRE))
                return false;

            if (string.IsNullOrEmpty(persona.PEPER_APELLIDO))
                return false;

            if (string.IsNullOrEmpty(persona.PEPER_CEDULA))
                return false;

            if (string.IsNullOrEmpty(persona.PEPER_EMAIL))
                return false;

            if (string.IsNullOrEmpty(persona.PESEX_ID)) 
                return false;

            if (persona.PEPEPER_FECH_INGR == null)
                return false;

            if (!esEdicion && persona.PEPER_CEDULA.Length < 6)
                return false;

            
            if (!CD_Personal.Instancia.ValidarCedulaUnica(persona.PEPER_CEDULA, esEdicion ? persona.PEPER_ID : null))
                return false;

            if (!CD_Personal.Instancia.ValidarEmailUnico(persona.PEPER_EMAIL, esEdicion ? persona.PEPER_ID : null))
                return false;

            return true;
        }
    }
}