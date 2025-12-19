using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using CapaDatos;
using CapaModelo;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            var respuesta = LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

            if (respuesta.estado)
            {
                FormsAuthentication.SetAuthCookie(XEUSU_NOMBRE, false);

                var usuarioDetalle = ObtenerUsuarioPorNombre(XEUSU_NOMBRE);
                if (usuarioDetalle.estado)
                {
                    // Guardar el objeto Usuario completo en sesión
                    Session["Usuario"] = usuarioDetalle.objeto;
                    Session["UsuarioID"] = usuarioDetalle.objeto.XEUSU_ID;
                    Session["UsuarioEstado"] = usuarioDetalle.objeto.XEUSU_ESTADO;

                    // Obtener las opciones del usuario según su rol
                    var opcionesRespuesta = ObtenerOpcionesPorRol(usuarioDetalle.objeto.XEROL_ID);
                    if (opcionesRespuesta.estado)
                    {
                        Session["OpcionesUsuario"] = opcionesRespuesta.objeto;
                    }
                    else
                    {
                        // Si no tiene opciones, guardar lista vacía
                        Session["OpcionesUsuario"] = new List<Opcion>();
                    }
                }
                else
                {
                    // Crear un objeto temporal si no se puede obtener del detalle
                    var tempUsuario = new Usuario
                    {
                        XEUSU_NOMBRE = XEUSU_NOMBRE,
                        XEUSU_ID = "TempID",
                        XEROL_ID = "TempRol"
                    };
                    Session["Usuario"] = tempUsuario;
                    Session["OpcionesUsuario"] = new List<Opcion>();
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = respuesta.mensaje;
            return View();
        }

        // GET: Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
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

                var resultadoTupla = CD_Usuario.Instancia.LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);
                int resultado = resultadoTupla.Item1;

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

        private Respuesta<List<Opcion>> ObtenerOpcionesPorRol(string XEROL_ID)
        {
            Respuesta<List<Opcion>> response = new Respuesta<List<Opcion>>();
            try
            {
                // Aquí necesitas implementar el método que obtenga las opciones del rol
                // Debe consultar la tabla xr_xerol_xeopc y luego unir con xeopc_opcion

                // Paso 1: Obtener los XEOPC_ID del rol desde xr_xerol_xeopc
                // Paso 2: Obtener los detalles de cada opción desde xeopc_opcion

                // Ejemplo temporal - REEMPLAZA ESTO CON TU IMPLEMENTACIÓN REAL:
                var listaOpciones = new List<Opcion>();

                // Simulando datos basados en lo que me mostraste:
                // Tu rol tiene: AC1, AC2, ACA, CRE, FIN, PER, SE1, SE2, SE3
                listaOpciones.Add(new Opcion { XEOPC_ID = "AC1", XEOPC_NOMBRE = "Administrar Carreras" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "AC2", XEOPC_NOMBRE = "Reporte de Carreras" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "ACA", XEOPC_NOMBRE = "Académico" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "CRE", XEOPC_NOMBRE = "Crear Personal" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "FIN", XEOPC_NOMBRE = "Finanzas" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "PER", XEOPC_NOMBRE = "Personal" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "SE1", XEOPC_NOMBRE = "Administrar Roles" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "SE2", XEOPC_NOMBRE = "Asignar Roles" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "SE3", XEOPC_NOMBRE = "Asignar Opciones a Roles" });
                listaOpciones.Add(new Opcion { XEOPC_ID = "SEG", XEOPC_NOMBRE = "Seguridad" });

                response.estado = true;
                response.objeto = listaOpciones;
                response.mensaje = "Opciones obtenidas correctamente";

                // IMPORTANTE: Reemplaza el código anterior con una llamada REAL a tu capa de datos
                // Ejemplo: var lista = CD_Rol.Instancia.ObtenerOpcionesPorRol(XEROL_ID);

            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
                response.objeto = new List<Opcion>();
            }
            return response;
        }

        // Clase interna para respuestas
        private class Respuesta<T>
        {
            public bool estado { get; set; }
            public string mensaje { get; set; }
            public T objeto { get; set; }
        }
    }
}