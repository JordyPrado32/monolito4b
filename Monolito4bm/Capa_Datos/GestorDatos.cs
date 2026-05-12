using System;
using System.Data.SqlClient;

namespace Capa_Datos
{
    public class GestorDatos
    {
        // Tu cadena de conexión real
        private string cadena = "Data Source=.;Initial Catalog=deberes_4to;User ID=clase4b;Password=clase4b;Encrypt=False";

        // Método para registrar usuario recibiendo variables sueltas
        public int InsertarUsuarioDatos(string nombres, string cedula, string correo, string pass, DateTime fechaNac, int edad, string nick, string cel, int rol, DateTime fechaCrea)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"INSERT INTO tbl_usuario 
                                (usu_nombres, usu_cedula, correo_electronico, contraseña, fecha_nacimiento, edad, usu_nickname, numero_celular, rol_id, fecha_creacion) 
                                VALUES 
                                (@nom, @ced, @correo, @pass, @fec, @edad, @nick, @cel, @rol, @fechacrea); 
                                SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@nom", nombres);
                cmd.Parameters.AddWithValue("@ced", cedula);
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Parameters.AddWithValue("@fec", fechaNac);
                cmd.Parameters.AddWithValue("@edad", edad);
                cmd.Parameters.AddWithValue("@nick", nick);
                cmd.Parameters.AddWithValue("@cel", cel);
                cmd.Parameters.AddWithValue("@rol", rol);
                cmd.Parameters.AddWithValue("@fechacrea", fechaCrea);

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()); // Devuelve el ID
            }
        }
    }
}