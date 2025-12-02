using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using System.Net.Mail;
using System.Security.Cryptography;

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

        public int LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            int respuesta = 0;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_LoginUsuario", oConexion);
                    cmd.Parameters.AddWithValue("XEUSU_NOMBRE", XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("XEUSU_CONTRA", XEUSU_CONTRA);
                    cmd.Parameters.Add("XEUSU_ID", SqlDbType.Char, 5).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    string idResult = cmd.Parameters["XEUSU_ID"].Value.ToString();
                    respuesta = string.IsNullOrEmpty(idResult) ? 0 : 1;

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
                SqlCommand cmd = new SqlCommand("usp_ObtenerDetalleUsuario", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@XEUSU_ID", XEUSU_ID);

                try
                {
                    oConexion.Open();
                    using (XmlReader dr = cmd.ExecuteXmlReader())
                    {
                        while (dr.Read())
                        {
                            XDocument doc = XDocument.Load(dr);
                            if (doc.Element("Usuario") != null)
                            {
                                rptUsuario = (from dato in doc.Elements("Usuario")
                                              select new Usuario()
                                              {
                                                  XEUSU_ID = dato.Element("XEUSU_ID").Value,
                                                  XEUSU_NOMBRE = dato.Element("XEUSU_NOMBRE").Value,
                                                  XEUSU_CONTRA = dato.Element("XEUSU_CONTRA").Value,
                                                  XEUSU_ESTADO = dato.Element("XEUSU_ESTADO").Value
                                              }).FirstOrDefault();
                            }
                            else
                            {
                                rptUsuario = null;
                            }
                        }

                        dr.Close();

                    }

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
                SqlCommand cmd = new SqlCommand("usp_ObtenerUsuario", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            XEUSU_ID = dr["XEUSU_ID"].ToString(),
                            XEUSU_NOMBRE = dr["XEUSU_NOMBRE"].ToString(),
                            XEUSU_CONTRA = dr["XEUSU_CONTRA"].ToString(),
                            XEUSU_ESTADO = dr["XEUSU_ESTADO"].ToString()
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
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                 
                    string contrasenaEncriptada = Encriptar(oUsuario.XEUSU_CONTRA);

                    SqlCommand cmd = new SqlCommand("usp_RegistrarUsuario", oConexion);

                    cmd.Parameters.AddWithValue("XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("XEUSU_CONTRA", contrasenaEncriptada); // Usar encriptada
                    cmd.Parameters.AddWithValue("XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oConexion.Open();

                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
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
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_ModificarUsuario", oConexion);
                    cmd.Parameters.AddWithValue("XEUSU_ID", oUsuario.XEUSU_ID);
                    cmd.Parameters.AddWithValue("XEUSU_NOMBRE", oUsuario.XEUSU_NOMBRE);
                    cmd.Parameters.AddWithValue("XEUSU_CONTRA", oUsuario.XEUSU_CONTRA);
                    cmd.Parameters.AddWithValue("XEUSU_ESTADO", oUsuario.XEUSU_ESTADO);

                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oConexion.Open();

                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);

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
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarUsuario", oConexion);
                    cmd.Parameters.AddWithValue("XEUSU_ID", XEUSU_ID);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);

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
            char remplaza;
            string re_incrementa = "";
            for (int i = 0; i < str.Length; i++)
            {
                remplaza = (char)((int)str[i] - 5);
                re_incrementa = re_incrementa + remplaza.ToString();
            }
            return re_incrementa;
        }

        public int CambiarClave(string XEUSU_NOMBRE, string XEUSU_CONTRA, string nuevaClave)
        {
            int res = 0;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                nuevaClave = Encriptar(nuevaClave);
                oConexion.Open();
                string consulta = "UPDATE XEUSU SET XEUSU_CONTRA = '" + nuevaClave + "' WHERE XEUSU_NOMBRE = '" + XEUSU_NOMBRE + "'";
                try
                {
                    SqlCommand cmd = new SqlCommand(consulta, oConexion);
                    cmd.ExecuteNonQuery();
                    res = 1;
                }
                catch (SqlException er)
                {
                    Console.WriteLine("Error en sentencia SQL: " + er);
                    return 0;
                }
            }
            return res;
        }
    }
}