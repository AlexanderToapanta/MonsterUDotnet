using CapaDatos;
using CapaModelo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Monster_University.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web;
using System.Web.Mvc;


namespace Monster_University.Controllers
{
    public class ControladorPersonalController : Controller
    {
        
        private readonly string _rutaBaseImagenes;

        public ControladorPersonalController()
        {
            // Ruta específica que indicaste
            _rutaBaseImagenes = @"C:\Users\DELL\Documents\PROYECTO_WEB_DOTNET\MonsterUDotnet\Monster_University\img";

            // Crear directorio si no existe
            if (!Directory.Exists(_rutaBaseImagenes))
            {
                Directory.CreateDirectory(_rutaBaseImagenes);
                System.Diagnostics.Debug.WriteLine($"✅ Directorio creado: {_rutaBaseImagenes}");
            }
        }
        // GET: ControladorPersonal/crearpersonal
        public ActionResult crearpersonal()
        {
            var model = new Personal();
            model.PEPEPER_FECH_INGR = DateTime.Now;

            // Cargar datos desde sus respectivas tablas
            ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
            ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();

            // Generar ID
            var nuevoId = GenerarIdPersonaAutomatico();
            ViewBag.IdGenerado = nuevoId;
            model.PEPER_ID = nuevoId;

            // Inicializar ruta de imágenes si no existe
            string rutaImagenes = @"C:\Users\DELL\Documents\PROYECTO_WEB_DOTNET\MonsterUDotnet\Monster_University\img";
            if (!Directory.Exists(rutaImagenes))
            {
                Directory.CreateDirectory(rutaImagenes);
                System.Diagnostics.Debug.WriteLine($"✅ Directorio de imágenes creado: {rutaImagenes}");
            }

            return View(model);
        }

        // POST: ControladorPersonal/crearpersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult crearpersonal(Personal model, HttpPostedFileBase imagenPersona)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INICIO CREAR PERSONAL ===");

                // Verificar que el modelo tenga ID
                if (string.IsNullOrEmpty(model.PEPER_ID))
                {
                    model.PEPER_ID = GenerarIdPersonaAutomatico();
                    System.Diagnostics.Debug.WriteLine($"🆔 ID generado automáticamente: {model.PEPER_ID}");
                }

                // 1. PROCESAR IMAGEN SI SE SUBIÓ
                if (imagenPersona != null && imagenPersona.ContentLength > 0)
                {
                    System.Diagnostics.Debug.WriteLine("📤 Procesando imagen subida...");

                    // Validar tipo de archivo
                    string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };
                    string extension = Path.GetExtension(imagenPersona.FileName).ToLower();

                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ViewBag.Error = "Solo se permiten imágenes JPG, JPEG, PNG o GIF";
                        ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                        ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();
                        return View(model);
                    }

                    // Validar tamaño (máximo 5MB)
                    if (imagenPersona.ContentLength > 5 * 1024 * 1024)
                    {
                        ViewBag.Error = "La imagen no debe superar los 5MB";
                        ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                        ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();
                        return View(model);
                    }

                    // Generar nombre único para la imagen
                    string cedulaLimpia = !string.IsNullOrEmpty(model.PEPER_CEDULA)
                        ? new string(model.PEPER_CEDULA.Where(char.IsDigit).ToArray())
                        : "temp_" + DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    string nombreArchivoImagen = $"{cedulaLimpia}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{extension}";
                    string rutaImagenes = @"C:\Users\DELL\Documents\PROYECTO_WEB_DOTNET\MonsterUDotnet\Monster_University\img";
                    string rutaCompleta = Path.Combine(rutaImagenes, nombreArchivoImagen);

                    // Asegurar que existe el directorio
                    if (!Directory.Exists(rutaImagenes))
                    {
                        Directory.CreateDirectory(rutaImagenes);
                    }

                    // Guardar la imagen
                    try
                    {
                        imagenPersona.SaveAs(rutaCompleta);
                        model.PEPER_FOTO = nombreArchivoImagen;
                        System.Diagnostics.Debug.WriteLine($"✅ Imagen guardada: {nombreArchivoImagen}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error guardando imagen: {ex.Message}");
                        // Continuar sin imagen
                        model.PEPER_FOTO = null;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ No se subió imagen o está vacía");
                    model.PEPER_FOTO = null;
                }

                // 2. VALIDAR DATOS DE LA PERSONA
                System.Diagnostics.Debug.WriteLine("🔍 Validando datos de la persona...");

                if (!ValidarDatosPersona(model))
                {
                    ViewBag.Error = "Datos inválidos. Revise los campos requeridos.";
                    ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                    ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();

                    // Regenerar ID para la vista
                    ViewBag.IdGenerado = model.PEPER_ID;

                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine("✅ Datos validados correctamente");

                // 3. GUARDAR PERSONA EN BASE DE DATOS
                System.Diagnostics.Debug.WriteLine("💾 Guardando persona en BD...");

                bool personaCreada = CD_Personal.Instancia.RegistrarPersonal(model);

                if (!personaCreada)
                {
                    ViewBag.Error = "Error al guardar la persona en la base de datos.";
                    ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                    ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();
                    ViewBag.IdGenerado = model.PEPER_ID;


                    // Si se guardó una imagen pero falló el registro, eliminarla
                    if (!string.IsNullOrEmpty(model.PEPER_FOTO))
                    {
                        try
                        {
                            string rutaImagen = Path.Combine(
                                @"C:\Users\DELL\Documents\PROYECTO_WEB_DOTNET\MonsterUDotnet\Monster_University\img",
                                model.PEPER_FOTO);
                            if (System.IO.File.Exists(rutaImagen))
                            {
                                System.IO.File.Delete(rutaImagen);
                            }
                        }
                        catch { }
                    }

                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Persona creada con ID: {model.PEPER_ID}");

                // 4. CREAR USUARIO AUTOMÁTICAMENTE
                System.Diagnostics.Debug.WriteLine("👤 Creando usuario automático...");

                var usuarioCreado = CrearUsuarioParaPersona(model);
                bool correoEnviado = false;

                if (usuarioCreado != null)
                {
                    // Actualizar persona con ID de usuario
                    model.XEUSU_ID = usuarioCreado.XEUSU_ID;
                    CD_Personal.Instancia.ModificarPersonal(model);

                    System.Diagnostics.Debug.WriteLine($"✅ Usuario creado con ID: {usuarioCreado.XEUSU_ID}");

                    // 5. ENVIAR CORREO CON CREDENCIALES (SÍNCRONO - como Java)
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("📧 Intentando enviar correo con credenciales...");
                        correoEnviado = EnviarCorreoCredencialesSincrono(model, usuarioCreado); // ← SÍNCRONO

                        if (correoEnviado)
                        {
                            System.Diagnostics.Debug.WriteLine("✅ Correo enviado exitosamente");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ No se pudo enviar el correo");
                        }
                    }
                    catch (Exception exEmail)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Error enviando correo: {exEmail.Message}");
                        // No fallamos por error en correo, solo registramos
                    }

                    // Mensaje según si se envió correo o no
                    string mensajeBase = $"✅ Persona creada con ID: {model.PEPER_ID} y Usuario creado con ID: {usuarioCreado.XEUSU_ID}";

                    if (correoEnviado)
                    {
                        TempData["SuccessMessage"] = $"{mensajeBase}<br/>📧 Las credenciales han sido enviadas al correo: {model.PEPER_EMAIL}";
                    }
                    else
                    {
                        TempData["WarningMessage"] = $"{mensajeBase}<br/>⚠️ Las credenciales NO se pudieron enviar por correo. Usuario: {GenerarNombreUsuario(model)}, Contraseña: {model.PEPER_CEDULA}";
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se pudo crear el usuario automático");
                    TempData["WarningMessage"] = $"Persona creada con ID: {model.PEPER_ID} pero no se pudo crear el usuario automático.";
                }

                // 6. REDIRECCIONAR PARA LIMPIAR FORMULARIO
                System.Diagnostics.Debug.WriteLine("🔄 Redirigiendo...");
                return RedirectToAction("crearpersonal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR en crearpersonal: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                ViewBag.Error = $"Error al crear persona: {ex.Message}";
                ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
                ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();

                // Regenerar ID para la vista
                if (string.IsNullOrEmpty(model.PEPER_ID))
                {
                    model.PEPER_ID = GenerarIdPersonaAutomatico();
                }
                ViewBag.IdGenerado = model.PEPER_ID;

                return View(model);
            }
        }

        // =====================================================================
        // MÉTODO SÍNCRONO PARA ENVIAR CORREO (igual que en Java)
        // =====================================================================
        private bool EnviarCorreoCredencialesSincrono(Personal persona, Usuario usuario)
        {
            try
            {
                // Verificar que el email no esté vacío
                if (string.IsNullOrWhiteSpace(persona.PEPER_EMAIL))
                {
                    System.Diagnostics.Debug.WriteLine("❌ No se puede enviar correo: Email vacío");
                    return false;
                }

                // Usar el EmailService
                var emailService = new EmailService();

                // Generar nombre de usuario y contraseña
                string nombreUsuario = GenerarNombreUsuario(persona);
                string contrasenia = persona.PEPER_CEDULA; // Contraseña = cédula

                System.Diagnostics.Debug.WriteLine($"📧 Enviando a: {persona.PEPER_EMAIL}");
                System.Diagnostics.Debug.WriteLine($"👤 Usuario generado: {nombreUsuario}");

                // Enviar correo SÍNCRONO (bloqueante)
                return emailService.EnviarCredencialesSincrono(
                    persona.PEPER_EMAIL,
                    nombreUsuario,
                    contrasenia
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en EnviarCorreoCredencialesSincrono: {ex.Message}");
                return false;
            }
        }
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

            // Cargar listas para los dropdowns
            ViewBag.Sexos = CD_Sexo.Instancia.ObtenerSexos();
            ViewBag.EstadosCiviles = CD_EstadoCivil.Instancia.ObtenerEstadosCiviles();

            return View(personal);
        }

        // POST: ControladorPersonal/editarpersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editarpersonal(Personal model, FormCollection form)
        {
            try
            {
                // Validar que el modelo tenga ID
                if (string.IsNullOrEmpty(model.PEPER_ID))
                {
                    TempData["ErrorMessage"] = "ID de personal requerido.";
                    return RedirectToAction("listapersonal");
                }

                // Asignar valores del formulario si no vienen en el modelo
                if (string.IsNullOrEmpty(model.PEESC_ID) && !string.IsNullOrEmpty(form["PEESC_ID"]))
                {
                    model.PEESC_ID = form["PEESC_ID"];
                }

                if (string.IsNullOrEmpty(model.PESEX_ID) && !string.IsNullOrEmpty(form["PESEX_ID"]))
                {
                    model.PESEX_ID = form["PESEX_ID"];
                }

                if (string.IsNullOrEmpty(model.PEPER_TIPO) && !string.IsNullOrEmpty(form["PEPER_TIPO"]))
                {
                    model.PEPER_TIPO = form["PEPER_TIPO"];
                }

                // Validar datos
                if (!ValidarDatosPersona(model, true))
                {
                    TempData["ErrorMessage"] = "Datos inválidos.";
                    return RedirectToAction("editarpersonal", new { id = model.PEPER_ID });
                }

                // Actualizar en base de datos
                bool resultado = CD_Personal.Instancia.ModificarPersonal(model);

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


        [HttpPost]
        public JsonResult SubirImagen(HttpPostedFileBase imagenSubida, string cedulaPersona)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUG: SUBIR IMAGEN ===");

                if (imagenSubida == null || imagenSubida.ContentLength == 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ ERROR: No hay imagen seleccionada");
                    return Json(new { success = false, message = "No hay imagen seleccionada" });
                }

                System.Diagnostics.Debug.WriteLine($"📤 Archivo seleccionado: {imagenSubida.FileName}");
                System.Diagnostics.Debug.WriteLine($"📏 Tamaño: {imagenSubida.ContentLength} bytes");

                // 1. Verificar cédula
                string cedula = "temp";
                if (!string.IsNullOrEmpty(cedulaPersona))
                {
                    // Remover caracteres no numéricos
                    cedula = new string(cedulaPersona.Where(char.IsDigit).ToArray());
                    System.Diagnostics.Debug.WriteLine($"📋 Cédula procesada: {cedula}");
                }
                else
                {
                    cedula = "temp_" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    System.Diagnostics.Debug.WriteLine("⚠️ ADVERTENCIA: No hay cédula, usando nombre temporal");
                }

                // 2. Generar nombre único seguro
                string nombreOriginal = Path.GetFileName(imagenSubida.FileName);
                string extension = Path.GetExtension(nombreOriginal);

                // Validar extensión permitida
                string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };
                if (!extensionesPermitidas.Contains(extension.ToLower()))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Solo se permiten imágenes JPG, JPEG, PNG o GIF"
                    });
                }

                // Validar tamaño máximo (5MB)
                if (imagenSubida.ContentLength > 5 * 1024 * 1024) // 5MB
                {
                    return Json(new
                    {
                        success = false,
                        message = "La imagen no debe superar los 5MB"
                    });
                }

                // Generar nombre único
                string nombreArchivoImagen = $"{cedula}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{extension}";

                // 3. Definir ruta EXACTA
                string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivoImagen);
                System.Diagnostics.Debug.WriteLine($"📁 RUTA COMPLETA: {rutaCompleta}");

                // 4. Crear directorio si no existe (redundante, pero seguro)
                Directory.CreateDirectory(_rutaBaseImagenes);

                // 5. Guardar archivo
                imagenSubida.SaveAs(rutaCompleta);
                System.Diagnostics.Debug.WriteLine($"✅ Bytes escritos: {new FileInfo(rutaCompleta).Length}");

                // 6. VERIFICAR que se guardó
                if (System.IO.File.Exists(rutaCompleta))
                {
                    System.Diagnostics.Debug.WriteLine("🎉 ARCHIVO GUARDADO EXITOSAMENTE");
                    var fileInfo = new FileInfo(rutaCompleta);
                    System.Diagnostics.Debug.WriteLine($"   Nombre: {fileInfo.Name}");
                    System.Diagnostics.Debug.WriteLine($"   Tamaño: {fileInfo.Length} bytes");
                    System.Diagnostics.Debug.WriteLine($"   Ruta: {fileInfo.FullName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ ERROR: El archivo NO se guardó");
                    return Json(new { success = false, message = "Error al guardar la imagen" });
                }

                // 7. Retornar información para asignar a la persona
                System.Diagnostics.Debug.WriteLine($"📝 Nombre asignado: {nombreArchivoImagen}");

                return Json(new
                {
                    success = true,
                    fileName = nombreArchivoImagen,
                    originalName = nombreOriginal
                });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR CRÍTICO: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /**
         * Método para eliminar la imagen
         */
        [HttpPost]
        public JsonResult EliminarImagen(string nombreArchivo)
        {
            try
            {
                // 1. Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(nombreArchivo))
                {
                    string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivo);

                    System.Diagnostics.Debug.WriteLine($"🗑️ Intentando eliminar: {rutaCompleta}");

                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                        System.Diagnostics.Debug.WriteLine($"✅ Imagen eliminada del servidor: {nombreArchivo}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ La imagen no existía en el servidor: {nombreArchivo}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("🔄 Referencias de imagen limpiadas");

                return Json(new { success = true, message = "Imagen eliminada" });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al eliminar imagen: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /**
         * Obtener URL de la imagen para mostrar
         */
        public string GetUrlImagen(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ GetUrlImagen: nombreArchivo es null/vacío");
                return null;
            }

            
            string url = Url.Action("MostrarImagen", "ControladorPersonal", new { nombre = nombreArchivo });

            // Verificar que el archivo existe físicamente
            string rutaFisica = Path.Combine(_rutaBaseImagenes, nombreArchivo);
            bool archivoExiste = System.IO.File.Exists(rutaFisica);
            System.Diagnostics.Debug.WriteLine($"📁 Archivo existe físicamente ({rutaFisica}): {archivoExiste}");

            return archivoExiste ? url : null;
        }

        /**
         * Método para mostrar imágenes desde la carpeta física
         */
        public ActionResult MostrarImagen(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return HttpNotFound();
            }

            string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombre);

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return HttpNotFound();
            }

            // Obtener el tipo MIME basado en la extensión
            string extension = Path.GetExtension(nombre).ToLower();
            string contentType = "image/jpeg"; // por defecto

            switch (extension)
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
            }

            return File(rutaCompleta, contentType);
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
                var listaPersonal = CD_Personal.Instancia.ObtenerPersonales();
                if (listaPersonal == null || listaPersonal.Count == 0)
                {
                    return "PE001";
                }

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

            if (nombreUsuario.Length > 100)
            {
                nombreUsuario = nombreUsuario.Substring(0, 100);
            }

            return nombreUsuario;
        }

        private bool ValidarDatosPersona(Personal persona, bool esEdicion = false)
        {
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

            if (string.IsNullOrEmpty(persona.PEPER_TIPO))
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
    
