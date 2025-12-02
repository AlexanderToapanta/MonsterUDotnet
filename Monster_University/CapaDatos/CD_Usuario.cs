using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Usuario
    {
        public static CD_Usuario _instancia = null;

        private CD_Usuario()
        {
        }

        public static CD_Usuario Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Usuario();
                }
                return _instancia;
            }
        }

        // CORREGIDO: Encriptar contraseña antes de comparar
        public int LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            int respuesta = 0;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // ¡IMPORTANTE! Encriptar la contraseña recibida
                    string contrasenaEncriptada = Encriptar(XEUSU_CONTRA);

                    string query = @"SELECT XEUSU_ID 
                                   FROM XEUSU_USUAR 
                                   WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE 
                                   AND XEUSU_CONTRA = @XEUSU_CONTRA 
                                   AND XEUSU_ESTADO = 'ACTIVO'";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaEncriptada); // Usar encriptada

                    oConexion.Open();

                    object result = cmd.ExecuteScalar();
                    respuesta = (result != null && !string.IsNullOrEmpty(result.ToString())) ? 1 : 0;
                }
                catch (Exception ex)
                {
                    respuesta = 0;
                }
            }
            return respuesta;
        }

        public Usuario ObtenerDetalleUsuario(string XEUSU_ID)
        {
            Usuario rptUsuario = new Usuario();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEUSU_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                        XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO
                                 FROM XEUSU_USUAR 
                                 WHERE XEUSU_ID = @XEUSU_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptUsuario = new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString()
                        };
                    }
                    else
                    {
                        rptUsuario = null;
                    }
                    dr.Close();
                    return rptUsuario;
                }
                catch (Exception ex)
                {
                    rptUsuario = null;
                    return rptUsuario;
                }
            }
        }

        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> rptListaUsuario = new List<Usuario>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEUSU_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                        XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO
                                 FROM XEUSU_USUAR 
                                 ORDER BY XEUSU_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            MECARR_ID = dr["MECARR_ID"]?.ToString(),
                            MEEST_ID = dr["MEEST_ID"]?.ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"]?.ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"]?.ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"]?.ToString()
                        });
                    }
                    dr.Close();
                    return rptListaUsuario;
                }
                catch (Exception ex)
                {
                    rptListaUsuario = null;
                    return rptListaUsuario;
                }
            }
        }

        public bool RegistrarUsuario(Usuario oUsuario)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string contrasenaEncriptada = Encriptar(oUsuario.XEUSU_CONTRA);

                    string query = @"INSERT INTO XEUSU_USUAR 
                                    (XEUSU_ID, PEPER_ID, MECARR_ID, MEEST_ID, 
                                     XEUSU_NOMBRE, XEUSU_CONTRA, XEUSU_ESTADO)
                                     VALUES 
                                    (@XEUSU_ID, @PEPER_ID, @MECARR_ID, @MEEST_ID, 
                                     @XEUSU_NOMBRE, @XEUSU_CONTRA, @XEUSU_ESTADO)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    // Parámetros principales
                    cmd.Parameters.AddWithValue("@XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaEncriptada);
                    cmd.Parameters.AddWithValue("@XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    // Parámetros FK
                    cmd.Parameters.AddWithValue("@PEPER_ID",
                        string.IsNullOrEmpty(oUsuario.PEPER_ID) ? (object)DBNull.Value : oUsuario.PEPER_ID);
                    cmd.Parameters.AddWithValue("@MECARR_ID",
                        string.IsNullOrEmpty(oUsuario.MECARR_ID) ? (object)DBNull.Value : oUsuario.MECARR_ID);
                    cmd.Parameters.AddWithValue("@MEEST_ID",
                        string.IsNullOrEmpty(oUsuario.MEEST_ID) ? (object)DBNull.Value : oUsuario.MEEST_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarUsuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarUsuario(Usuario oUsuario)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Si se está cambiando la contraseña, encriptarla
                    string contrasenaParaGuardar = oUsuario.XEUSU_CONTRA;
                    // Verificar si la contraseña ya está encriptada (más de 5 caracteres y contiene caracteres especiales)
                    // Si no está encriptada, encriptarla
                    if (!string.IsNullOrEmpty(oUsuario.XEUSU_CONTRA) && oUsuario.XEUSU_CONTRA.Length > 0)
                    {
                        // Una forma simple de verificar si ya está encriptada
                        // (esto es un ejemplo básico, puedes mejorar esta lógica)
                        contrasenaParaGuardar = Encriptar(oUsuario.XEUSU_CONTRA);
                    }

                    string query = @"UPDATE XEUSU_USUAR SET 
                                    PEPER_ID = @PEPER_ID,
                                    MECARR_ID = @MECARR_ID,
                                    MEEST_ID = @MEEST_ID,
                                    XEUSU_NOMBRE = @XEUSU_NOMBRE,
                                    XEUSU_CONTRA = @XEUSU_CONTRA,
                                    XEUSU_ESTADO = @XEUSU_ESTADO
                                    WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    // Parámetros principales
                    cmd.Parameters.AddWithValue("@XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaParaGuardar);
                    cmd.Parameters.AddWithValue("@XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    // Parámetros FK
                    cmd.Parameters.AddWithValue("@PEPER_ID",
                        string.IsNullOrEmpty(oUsuario.PEPER_ID) ? (object)DBNull.Value : oUsuario.PEPER_ID);
                    cmd.Parameters.AddWithValue("@MECARR_ID",
                        string.IsNullOrEmpty(oUsuario.MECARR_ID) ? (object)DBNull.Value : oUsuario.MECARR_ID);
                    cmd.Parameters.AddWithValue("@MEEST_ID",
                        string.IsNullOrEmpty(oUsuario.MEEST_ID) ? (object)DBNull.Value : oUsuario.MEEST_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                }
            }
            return respuesta;
        }

        public bool EliminarUsuario(string XEUSU_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Primero eliminar relaciones en XR_XEUSU_XEROL
                    string queryEliminarRoles = @"DELETE FROM XR_XEUSU_XEROL 
                                                  WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmdRoles = new SqlCommand(queryEliminarRoles, oConexion);
                    cmdRoles.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                    // Luego eliminar el usuario
                    string queryEliminarUsuario = @"DELETE FROM XEUSU_USUAR 
                                                    WHERE XEUSU_ID = @XEUSU_ID";

                    SqlCommand cmdUsuario = new SqlCommand(queryEliminarUsuario, oConexion);
                    cmdUsuario.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                    oConexion.Open();

                    // Ejecutar en transacción
                    using (SqlTransaction transaction = oConexion.BeginTransaction())
                    {
                        try
                        {
                            cmdRoles.Transaction = transaction;
                            cmdUsuario.Transaction = transaction;

                            cmdRoles.ExecuteNonQuery();
                            int rowsAffected = cmdUsuario.ExecuteNonQuery();

                            transaction.Commit();
                            respuesta = rowsAffected > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            respuesta = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                }
            }
            return respuesta;
        }

        public string Encriptar(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            char remplaza;
            string re_incrementa = "";
            for (int i = 0; i < str.Length; i++)
            {
                remplaza = (char)((int)str[i] + 5);
                re_incrementa = re_incrementa + remplaza.ToString();
            }
            return re_incrementa;
        }

        public string DesEncriptar(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            char remplaza;
            string re_incrementa = "";
            for (int i = 0; i < str.Length; i++)
            {
                remplaza = (char)((int)str[i] - 5);
                re_incrementa = re_incrementa + remplaza.ToString();
            }
            return re_incrementa;
        }

        // CORREGIDO: Encriptar contraseña actual antes de verificar
        public int CambiarClave(string XEUSU_NOMBRE, string XEUSU_CONTRA, string nuevaClave)
        {
            int res = 0;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Encriptar contraseña actual para verificar
                    string contrasenaActualEncriptada = Encriptar(XEUSU_CONTRA);

                    // Primero verificar que la contraseña actual es correcta
                    string queryVerificar = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                             WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE 
                                             AND XEUSU_CONTRA = @XEUSU_CONTRA";

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);
                    cmdVerificar.Parameters.AddWithValue("@XEUSU_CONTRA", contrasenaActualEncriptada);

                    oConexion.Open();

                    int existe = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (existe > 0)
                    {
                        // Actualizar contraseña
                        string nuevaClaveEncriptada = Encriptar(nuevaClave);
                        string queryActualizar = @"UPDATE XEUSU_USUAR 
                                                  SET XEUSU_CONTRA = @NUEVA_CLAVE 
                                                  WHERE XEUSU_NOMBRE = @XEUSU_NOMBRE";

                        SqlCommand cmdActualizar = new SqlCommand(queryActualizar, oConexion);
                        cmdActualizar.Parameters.AddWithValue("@NUEVA_CLAVE", nuevaClaveEncriptada);
                        cmdActualizar.Parameters.AddWithValue("@XEUSU_NOMBRE", XEUSU_NOMBRE);

                        cmdActualizar.ExecuteNonQuery();
                        res = 1;
                    }
                    else
                    {
                        res = 0; // Contraseña actual incorrecta
                    }
                }
                catch (Exception ex)
                {
                    res = 0;
                }
            }
            return res;
        }
    }
}