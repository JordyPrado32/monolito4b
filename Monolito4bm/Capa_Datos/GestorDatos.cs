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
        private string cadena = "Data Source=.;Initial Catalog=deberes_4to;Integrated Security=True;";

        // 1. OBTENER ROLES REALES DE LA BD
        public DataTable ObtenerRolesDatos()
        // ─────────────────────────────────────────────
        // REGISTRO DE USUARIO (ya existía)
        // ─────────────────────────────────────────────
        public int InsertarUsuarioDatos(string nombres, string cedula, string correo,
            string pass, DateTime fechaNac, int edad, string nick, string cel,
            int rol, DateTime fechaCrea)
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
                    (usu_nombres, usu_cedula, correo_electronico, contraseña,
                     fecha_nacimiento, edad, usu_nickname, numero_celular, rol_id, fecha_creacion) 
                    VALUES 
                    (@nom,@ced,@correo,@pass,@fec,@edad,@nick,@cel,@rol,@fechacrea); 
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
                cmd.Parameters.AddWithValue("@fechacrea", fechaCrea);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ─────────────────────────────────────────────
        // LOGIN: obtiene datos del usuario por nickname
        // ─────────────────────────────────────────────
        public DataTable ObtenerUsuarioPorNick(string nick)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"SELECT usu_id, contraseña, rol_id, intentos_fallidos,
                                        correo_electronico, usu_nombres, clave_temporal
                                 FROM   tbl_usuario
                                 WHERE  usu_nickname = @nick";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@nick", nick);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                con.Open();
                da.Fill(dt);
                return dt;
            }
        }

        // ─────────────────────────────────────────────
        // RECUPERACIÓN: busca por nickname O correo
        // Devuelve: usu_id, usu_nombres, numero_celular
        // ─────────────────────────────────────────────
        public DataTable ObtenerUsuarioPorNickOCorreo(string valor)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"SELECT usu_id, usu_nombres, numero_celular
                                 FROM   tbl_usuario
                                 WHERE  usu_nickname        = @valor
                                    OR  correo_electronico  = @valor";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@valor", valor);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                con.Open();
                da.Fill(dt);
                return dt;
            }
        }

        // ─────────────────────────────────────────────
        // Guarda clave temporal hasheada + flag en BD
        // ─────────────────────────────────────────────
        public void GuardarClaveTemporal(int usuId, string hashClaveTemporal)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET    contraseña     = @hash,
                                        clave_temporal = 1
                                 WHERE  usu_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@hash", hashClaveTemporal);
                cmd.Parameters.AddWithValue("@id", usuId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Actualiza contraseña definitiva y quita flag
        // ─────────────────────────────────────────────
        public void ActualizarContrasena(int usuId, string hashNuevaClave)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET    contraseña     = @hash,
                                        clave_temporal = 0
                                 WHERE  usu_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@hash", hashNuevaClave);
                cmd.Parameters.AddWithValue("@id", usuId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Suma 1 intento fallido
        // ─────────────────────────────────────────────
        public void IncrementarIntentosFallidos(int usuId)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET intentos_fallidos = intentos_fallidos + 1
                                 WHERE usu_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", usuId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Resetea intentos a 0
        // ─────────────────────────────────────────────
        public void ResetearIntentos(int usuId)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET intentos_fallidos = 0
                                 WHERE usu_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", usuId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Resetea intentos caducados (<=2 tras 24h)
        // ─────────────────────────────────────────────
        public void ResetearIntentosCaducados()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET    intentos_fallidos = 0
                                 WHERE  intentos_fallidos > 0
                                   AND  intentos_fallidos <= 2
                                   AND  DATEDIFF(HOUR, fecha_otp_generado, GETDATE()) >= 24";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Guarda OTP para el QR de login
        // ─────────────────────────────────────────────
        public void GuardarOtp(int usuId, string otp, DateTime generado)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET    codigo_otp         = @otp,
                                        fecha_otp_generado = @gen
                                 WHERE  usu_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@otp", otp);
                cmd.Parameters.AddWithValue("@gen", generado);
                cmd.Parameters.AddWithValue("@id", usuId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────────
        // Valida OTP (válido 5 minutos)
        // Devuelve rol_id o -1 si falla
        // ─────────────────────────────────────────────
        public int ValidarOtp(string otp)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"SELECT rol_id
                                 FROM   tbl_usuario
                                 WHERE  codigo_otp = @otp
                                   AND  DATEDIFF(MINUTE, fecha_otp_generado, GETDATE()) <= 5";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@otp", otp);
                con.Open();
                object resultado = cmd.ExecuteScalar();
                return resultado != null ? Convert.ToInt32(resultado) : -1;
            }
        }

        // ─────────────────────────────────────────────
        // Invalida el OTP tras usarlo
        // ─────────────────────────────────────────────
        public void InvalidarOtp(string otp)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"UPDATE tbl_usuario
                                 SET codigo_otp = NULL
                                 WHERE codigo_otp = @otp";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@otp", otp);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}