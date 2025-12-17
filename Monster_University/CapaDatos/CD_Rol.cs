using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Rol
    {
        public static CD_Rol _instancia = null;

        private CD_Rol()
        {
        }

        public static CD_Rol Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Rol();
                }
                return _instancia;
            }
        }

        public List<Rol> ObtenerRoles()
        {
            List<Rol> rptListaRoles = new List<Rol>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEROL_ID, XEROL_NOMBRE, XEROL_DESCRI 
                                   FROM XEROL_ROL 
                                   ORDER BY XEROL_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaRoles.Add(new Rol()
                        {
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            XEROL_NOMBRE = dr["XEROL_NOMBRE"]?.ToString(),
                            XEROL_DESCRI = dr["XEROL_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                    return rptListaRoles;
                }
                catch (Exception ex)
                {
                    rptListaRoles = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerRoles: " + ex.Message);
                    return rptListaRoles;
                }
            }
        }

        public Rol ObtenerDetalleRol(string XEROL_ID)
        {
            Rol rptRol = new Rol();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEROL_ID, XEROL_NOMBRE, XEROL_DESCRI
                                   FROM XEROL_ROL 
                                   WHERE XEROL_ID = @XEROL_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptRol = new Rol()
                        {
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            XEROL_NOMBRE = dr["XEROL_NOMBRE"]?.ToString(),
                            XEROL_DESCRI = dr["XEROL_DESCRI"]?.ToString()
                        };
                    }
                    else
                    {
                        rptRol = null;
                    }
                    dr.Close();
                    return rptRol;
                }
                catch (Exception ex)
                {
                    rptRol = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerDetalleRol: " + ex.Message);
                    return rptRol;
                }
            }
        }

        public bool RegistrarRol(Rol oRol)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"INSERT INTO XEROL_ROL 
                                         (XEROL_ID, XEROL_NOMBRE, XEROL_DESCRI) 
                                         VALUES 
                                         (@XEROL_ID, @XEROL_NOMBRE, @XEROL_DESCRI)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@XEROL_ID", oRol.XEROL_ID);
                    cmd.Parameters.AddWithValue("@XEROL_NOMBRE", oRol.XEROL_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEROL_DESCRI",
                        string.IsNullOrEmpty(oRol.XEROL_DESCRI) ? (object)DBNull.Value : oRol.XEROL_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarRol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarRol(Rol oRol)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE XEROL_ROL SET 
                                         XEROL_NOMBRE = @XEROL_NOMBRE,
                                         XEROL_DESCRI = @XEROL_DESCRI
                                         WHERE XEROL_ID = @XEROL_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    cmd.Parameters.AddWithValue("@XEROL_ID", oRol.XEROL_ID);
                    cmd.Parameters.AddWithValue("@XEROL_NOMBRE", oRol.XEROL_NOMBRE);
                    cmd.Parameters.AddWithValue("@XEROL_DESCRI",
                        string.IsNullOrEmpty(oRol.XEROL_DESCRI) ? (object)DBNull.Value : oRol.XEROL_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarRol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EliminarRol(string XEROL_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Verificar si hay usuarios con este rol
                    string queryVerificar = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                               WHERE XEROL_ID = @XEROL_ID";

                    string queryEliminar = @"DELETE FROM XEROL_ROL 
                                               WHERE XEROL_ID = @XEROL_ID";

                    oConexion.Open();

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                    int usuariosRelacionados = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (usuariosRelacionados > 0)
                    {
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("No se puede eliminar rol con usuarios asignados.");
                    }
                    else
                    {
                        SqlCommand cmdEliminar = new SqlCommand(queryEliminar, oConexion);
                        cmdEliminar.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);

                        int rowsAffected = cmdEliminar.ExecuteNonQuery();
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarRol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public List<Rol> BuscarRol(string criterio, string valor)
        {
            List<Rol> rptListaRoles = new List<Rol>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"SELECT XEROL_ID, XEROL_NOMBRE, XEROL_DESCRI
                                       FROM XEROL_ROL 
                                       WHERE ";

                    switch (criterio.ToUpper())
                    {
                        case "ID":
                            query += "XEROL_ID LIKE @VALOR";
                            break;
                        case "NOMBRE":
                            query += "XEROL_NOMBRE LIKE @VALOR";
                            break;
                        case "DESCRIPCION":
                            query += "XEROL_DESCRI LIKE @VALOR";
                            break;
                        default:
                            query += "XEROL_NOMBRE LIKE @VALOR";
                            break;
                    }

                    query += " ORDER BY XEROL_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@VALOR", "%" + valor + "%");

                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaRoles.Add(new Rol()
                        {
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            XEROL_NOMBRE = dr["XEROL_NOMBRE"]?.ToString(),
                            XEROL_DESCRI = dr["XEROL_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    rptListaRoles = null;
                    System.Diagnostics.Debug.WriteLine("Error en BuscarRol: " + ex.Message);
                }
            }
            return rptListaRoles;
        }

        public bool ValidarNombreRolUnico(string XEROL_NOMBRE, string XEROL_ID_EXCLUIR = null)
        {
            bool esUnico = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(XEROL_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM XEROL_ROL 
                                   WHERE XEROL_NOMBRE = @XEROL_NOMBRE";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEROL_NOMBRE", XEROL_NOMBRE);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM XEROL_ROL 
                                   WHERE XEROL_NOMBRE = @XEROL_NOMBRE 
                                   AND XEROL_ID != @XEROL_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEROL_NOMBRE", XEROL_NOMBRE);
                        cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnico = count == 0;
                }
                catch (Exception ex)
                {
                    esUnico = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarNombreRolUnico: " + ex.Message);
                }
            }
            return esUnico;
        }
    }
}