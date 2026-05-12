using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Capa_Datos
{
    public class GestorDatos
    {
        // Lee la conexión del Web.config de la Presentación
        private string obtenerCadena()
        {
            return ConfigurationManager.ConnectionStrings["Capa_Datos.Properties.Settings.deberes_4toConnectionString"].ConnectionString;
        }

        // 1. OBTENER ROLES REALES DE LA BD
        public DataTable ObtenerRolesDatos()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(obtenerCadena()))
            {
                string query = "SELECT rol_id, nombre_rol FROM tbl_rol"; // Tu tabla real
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(dt);
            }
            return dt;
        }

        // 2. INSERTAR USUARIO (Recibe datos sueltos)
        public int InsertarUsuarioDatos(string nombres, string cedula, string correo, string pass, DateTime fechaNac, string nick, string cel, int rol, DateTime fechaCrea)
        {
            using (SqlConnection con = new SqlConnection(obtenerCadena()))
            {
                // QUITAMOS 'edad' y '@edad' del query
                string query = @"INSERT INTO tbl_usuario 
                        (usu_nombres, usu_cedula, correo_electronico, contraseña, fecha_nacimiento, usu_nickname, numero_celular, rol_id, fecha_creacion) 
                        VALUES 
                        (@nom, @ced, @correo, @pass, @fecN, @nick, @cel, @rol, @fecC); 
                        SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@nom", nombres);
                cmd.Parameters.AddWithValue("@ced", cedula);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Parameters.AddWithValue("@fecN", fechaNac);
                cmd.Parameters.AddWithValue("@nick", nick);
                cmd.Parameters.AddWithValue("@cel", cel);
                cmd.Parameters.AddWithValue("@rol", rol);
                cmd.Parameters.AddWithValue("@fecC", fechaCrea);

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        // 3. INSERTAR FOTO
        public void InsertarFotoDatos(int usuId, string nombre, string tipo, byte[] archivo, DateTime fechaSub, bool principal)
        {
            using (SqlConnection con = new SqlConnection(obtenerCadena()))
            {
                string query = @"INSERT INTO tbl_usuario_fotos 
                                (usu_id, nombre_archivo, content_type, foto, fecha_subida, es_principal) 
                                VALUES (@uid, @nom, @tip, @fot, @fec, @pri)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@uid", usuId);
                cmd.Parameters.AddWithValue("@nom", nombre);
                cmd.Parameters.AddWithValue("@tip", tipo);
                cmd.Parameters.AddWithValue("@fot", archivo);
                cmd.Parameters.AddWithValue("@fec", fechaSub);
                cmd.Parameters.AddWithValue("@pri", principal);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}