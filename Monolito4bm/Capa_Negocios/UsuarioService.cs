using Capa_Datos;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Data;

namespace Capa_Negocios
{
    public enum ResultadoLogin
    {
        Exitoso,
        ExitosoClaveTemporalActiva,   // redirige a CambiarClave
        CredencialesInvalidas,
        UsuarioBloqueado,
        ErrorInterno
    }

    public enum ResultadoRecuperacion
    {
        Exitoso,
        UsuarioNoEncontrado,
        ErrorInterno
    }

    public class UsuarioService
    {
        private readonly GestorDatos _gestor = new GestorDatos();

        // ══════════════════════════════════════════
        // CONFIGURACIÓN SMTP
        // ══════════════════════════════════════════
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPuerto = 587;
        private const string SmtpUsuario = "jordypm180806@gmail.com";  // ← cambia
        private const string SmtpPassword = "tyup broi fmhq hymn";    // ← cambia
        private const string RemitenteNombre = "Sistema Login";

        // ══════════════════════════════════════════
        // CONFIGURACIÓN TWILIO
        // ══════════════════════════════════════════
        private const string TwilioAccountSid = "AC928d2c6233dd3219ed81a939ac0e5943"; // ← cambia
        private const string TwilioAuthToken = "8331776ac19e69dc419939418ba22731";   // ← cambia
        private const string TwilioNumero = "+14155238886";                       // ← tu número Twilio

        // ══════════════════════════════════════════════════════════════
        // ░░  SECCIÓN 1 — REGISTRO  (código original, sin cambios)  ░░
        // ══════════════════════════════════════════════════════════════

        public void RegistrarUsuario(tbl_usuario nuevoUsuario, string nombresSeparados,
            string apellidosSeparados, List<tbl_usuario_fotos> fotosAdjuntas)
        {
            var arrNombres = nombresSeparados.Trim().Split(' ');
            var arrApellidos = apellidosSeparados.Trim().Split(' ');

            if (arrNombres.Length < 2 || arrNombres.Any(n => n.Length < 3) ||
                arrApellidos.Length < 2 || arrApellidos.Any(a => a.Length < 3))
                throw new ArgumentException(
                    "Debe ingresar dos nombres y dos apellidos de mínimo 3 letras cada uno.");

            nuevoUsuario.usu_nombres = $"{nombresSeparados} {apellidosSeparados}";

            if (!ValidarCedulaEcuatoriana(nuevoUsuario.usu_cedula))
                throw new ArgumentException("Cédula ecuatoriana inválida.");

            if (nuevoUsuario.numero_celular.Length != 10 ||
                !nuevoUsuario.numero_celular.All(char.IsDigit))
                throw new ArgumentException("El número de celular debe tener exactamente 10 dígitos.");

            int edadCalculada = CalcularEdad(nuevoUsuario.fecha_nacimiento);
            if (edadCalculada < 18)
                throw new ArgumentException("El usuario debe ser mayor de 18 años.");
            nuevoUsuario.edad = edadCalculada;

            if (!Regex.IsMatch(nuevoUsuario.correo_electronico,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException(
                    "El correo electrónico no tiene un formato válido (@dominio.com).");

            if (nuevoUsuario.contraseña.Length < 3)
                throw new ArgumentException("La contraseña debe tener mínimo 3 caracteres.");

            nuevoUsuario.contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.contraseña);
            nuevoUsuario.usu_nickname = GenerarNickname(arrNombres, arrApellidos,
                                                        nuevoUsuario.usu_cedula);
            nuevoUsuario.fecha_creacion = DateTime.Now;
            nuevoUsuario.rol_id = 2;

            if (fotosAdjuntas.Count < 3 || fotosAdjuntas.Count > 5)
                throw new ArgumentException(
                    "Debe subir un mínimo de 3 y un máximo de 5 fotografías.");

            bool primera = true;
            foreach (var foto in fotosAdjuntas)
            {
                foto.fecha_subida = DateTime.Now;
                foto.es_principal = primera;
                primera = false;
                nuevoUsuario.Fotos.Add(foto);
            }
            // _gestor.InsertarUsuario(nuevoUsuario);
        }

        // ══════════════════════════════════════════════════════════════
        // ░░  SECCIÓN 2 — LOGIN MFA                                 ░░
        // ══════════════════════════════════════════════════════════════

        public ResultadoLogin IniciarSesion(string nick, string passwordIngresada,
            out int rolId, out string correo, out string mensajeErr)
        {
            rolId = 0;
            correo = string.Empty;
            mensajeErr = string.Empty;

            try
            {
                _gestor.ResetearIntentosCaducados();

                var dt = _gestor.ObtenerUsuarioPorNick(nick);
                if (dt.Rows.Count == 0)
                {
                    mensajeErr = "Usuario o contraseña incorrectos.";
                    return ResultadoLogin.CredencialesInvalidas;
                }

                var row = dt.Rows[0];
                int usuId = Convert.ToInt32(row["usu_id"]);
                string hashBd = row["contraseña"].ToString();
                int intentos = Convert.ToInt32(row["intentos_fallidos"]);
                rolId = Convert.ToInt32(row["rol_id"]);
                correo = row["correo_electronico"].ToString();
                string nombre = row["usu_nombres"].ToString();
                bool esTemporal = Convert.ToBoolean(row["clave_temporal"]);

                if (intentos >= 3)
                {
                    mensajeErr = "Cuenta bloqueada por demasiados intentos fallidos. " +
                                 "Contacte al administrador.";
                    return ResultadoLogin.UsuarioBloqueado;
                }

                if (!BCrypt.Net.BCrypt.Verify(passwordIngresada, hashBd))
                {
                    _gestor.IncrementarIntentosFallidos(usuId);
                    int restantes = 2 - intentos;
                    mensajeErr = restantes <= 0
                        ? "Contraseña incorrecta. Cuenta bloqueada."
                        : $"Contraseña incorrecta. Te quedan {restantes} intento(s).";
                    return ResultadoLogin.CredencialesInvalidas;
                }

                _gestor.ResetearIntentos(usuId);

                // Si tiene clave temporal, no envía QR: va directo a cambiar clave
                if (esTemporal)
                    return ResultadoLogin.ExitosoClaveTemporalActiva;

                string otp = GenerarOtp();
                _gestor.GuardarOtp(usuId, otp, DateTime.Now);
                EnviarQrPorCorreo(correo, nombre, otp);

                return ResultadoLogin.Exitoso;
            }
            catch (SqlException sqlEx)
            {
                // 1. LOGUEAR AQUÍ para el desarrollador (puedes usar un archivo o Debug)
                System.Diagnostics.Debug.WriteLine("SQL Error: " + sqlEx.Message);
                System.Diagnostics.Debug.WriteLine("Severidad: " + sqlEx.Class);
                System.Diagnostics.Debug.WriteLine("Stack: " + sqlEx.StackTrace);

                // 2. Respuesta amigable para el usuario
                mensajeErr = "Error de conexión con la base de datos. Intente más tarde.";
                return ResultadoLogin.ErrorInterno;
            }
            catch (Exception ex)
            {
                mensajeErr = "Error interno: " + ex.Message;
                return ResultadoLogin.ErrorInterno;
            }
        }

        public int ValidarOtp(string otp)
        {
            int rol = _gestor.ValidarOtp(otp);
            if (rol > 0)
                _gestor.InvalidarOtp(otp);
            return rol;
        }

        // ══════════════════════════════════════════════════════════════
        // ░░  SECCIÓN 3 — RECUPERACIÓN DE CLAVE                    ░░
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca al usuario por nickname o correo, genera una clave temporal,
        /// la guarda hasheada en BD y envía el SMS con Twilio.
        /// </summary>
        public ResultadoRecuperacion RecuperarClave(string nickOCorreo, out string mensajeErr)
        {
            mensajeErr = string.Empty;

            try
            {
                var dt = _gestor.ObtenerUsuarioPorNickOCorreo(nickOCorreo);

                if (dt.Rows.Count == 0)
                {
                    mensajeErr = "No se encontró ningún usuario con ese nickname o correo.";
                    return ResultadoRecuperacion.UsuarioNoEncontrado;
                }

                var row = dt.Rows[0];
                int usuId = Convert.ToInt32(row["usu_id"]);
                string nombre = row["usu_nombres"].ToString();
                string celular = row["numero_celular"].ToString();
                string correo = row["correo_electronico"].ToString();

                // Generar clave temporal legible (10 chars)
                string claveTemporal = GenerarClaveTemporal();

                // Guardar hash en BD con flag clave_temporal = 1
                string hash = BCrypt.Net.BCrypt.HashPassword(claveTemporal);
                _gestor.GuardarClaveTemporal(usuId, hash);

                string nombreCorto = ObtenerPrimerNombreApellido(nombre);
                bool envioPorCorreo = PareceCorreo(nickOCorreo);
                bool enviado = false;
                string detalleEnvio = string.Empty;

                if (envioPorCorreo)
                {
                    if (string.IsNullOrWhiteSpace(correo))
                        throw new InvalidOperationException("El usuario no tiene un correo electrónico registrado.");

                    EnviarClaveTemporalPorCorreo(correo, nombreCorto, claveTemporal);
                    enviado = true;
                }
                else if (TieneCelularValido(celular))
                {
                    try
                    {
                        EnviarSmsTwilio(celular, nombreCorto, claveTemporal);
                        enviado = true;
                    }
                    catch (Exception exSms)
                    {
                        detalleEnvio = exSms.Message;
                    }
                }

                if (!enviado && !string.IsNullOrWhiteSpace(correo))
                {
                    EnviarClaveTemporalPorCorreo(correo, nombreCorto, claveTemporal);
                    enviado = true;
                }

                if (!enviado)
                {
                    throw new InvalidOperationException(
                        string.IsNullOrWhiteSpace(detalleEnvio)
                            ? "No fue posible enviar la clave temporal al usuario."
                            : "No fue posible enviar la clave temporal. " + detalleEnvio);
                }

                return ResultadoRecuperacion.Exitoso;
            }
            catch (Exception ex)
            {
                mensajeErr = "Error interno: " + ex.Message;
                return ResultadoRecuperacion.ErrorInterno;
            }
        }

        /// <summary>
        /// Cambia la contraseña del usuario autenticado y quita el flag temporal.
        /// </summary>
        public void CambiarContrasena(int usuId, string nuevaClave)
        {
            if (nuevaClave.Length < 3)
                throw new ArgumentException("La nueva contraseña debe tener mínimo 3 caracteres.");

            string hash = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
            _gestor.ActualizarContrasena(usuId, hash);
        }

        public DataTable ObtenerEstadoCuentas()
        {
            return _gestor.ObtenerEstadoCuentas();
        }

        public void ResetearIntentosUsuario(int usuId)
        {
            var dt = _gestor.ObtenerUsuarioPorId(usuId);
            if (dt.Rows.Count == 0)
                throw new InvalidOperationException("No se encontró la cuenta seleccionada.");

            _gestor.ResetearIntentosAdmin(usuId);
        }

        public void DesbloquearCuentaConClaveTemporal(int usuId, out string correoDestino, out string claveTemporal)
        {
            correoDestino = string.Empty;
            claveTemporal = string.Empty;

            var dt = _gestor.ObtenerUsuarioPorId(usuId);
            if (dt.Rows.Count == 0)
                throw new InvalidOperationException("No se encontró la cuenta seleccionada.");

            var row = dt.Rows[0];
            string nombre = row["usu_nombres"].ToString();
            string correo = row["correo_electronico"].ToString();

            if (string.IsNullOrWhiteSpace(correo))
                throw new InvalidOperationException("La cuenta no tiene un correo registrado para enviar la clave temporal.");

            claveTemporal = GenerarClaveTemporal();
            string hash = BCrypt.Net.BCrypt.HashPassword(claveTemporal);

            _gestor.GuardarClaveTemporal(usuId, hash);
            _gestor.ResetearIntentosAdmin(usuId);

            string nombreCorto = ObtenerPrimerNombreApellido(nombre);
            EnviarClaveTemporalPorCorreo(correo, nombreCorto, claveTemporal);
            correoDestino = correo;
        }

        // ══════════════════════════════════════════════════════════════
        // ░░  SECCIÓN 4 — MÉTODOS AUXILIARES                        ░░
        // ══════════════════════════════════════════════════════════════

        // ── Originales ───────────────────────────────────────────────

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            int edad = DateTime.Today.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;
            return edad;
        }

        private string GenerarNickname(string[] n, string[] a, string cedula)
        {
            char c1 = n[0][0];
            char c2 = a[0][0];
            char c3 = a[1].Length > 1 ? a[1][1] : a[1][0];
            char c4 = n[1].Length > 1 ? n[1][1] : n[1][0];

            string letras = $"{c1}{c2}{c3}{c4}";
            Random rnd = new Random();

            var letrasMezcladas = letras
                .Select(c => rnd.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c))
                .ToArray();

            string simbolos = "@#$*&";
            char simbolo = simbolos[rnd.Next(simbolos.Length)];
            char num1 = cedula[rnd.Next(cedula.Length)];
            char num2 = cedula[rnd.Next(cedula.Length)];

            return $"{simbolo}{new string(letrasMezcladas)}{num1}{num2}";
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 ||
                !cedula.All(char.IsDigit)) return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24) return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito >= 6) return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
                suma += valor > 9 ? valor - 9 : valor;
            }

            int digitoVerificador = int.Parse(cedula[9].ToString());
            int decenaSuperior = (suma + 9) / 10 * 10;
            int calculado = decenaSuperior - suma;
            if (calculado == 10) calculado = 0;

            return calculado == digitoVerificador;
        }

        // ── Nuevos ───────────────────────────────────────────────────

        private string GenerarOtp()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            byte[] data = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(data);

            var sb = new StringBuilder(8);
            foreach (byte b in data)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }

        /// <summary>
        /// Genera clave temporal de 10 chars: letras + números, fácil de leer en SMS.
        /// Sin caracteres ambiguos (0/O, 1/l/I).
        /// </summary>
        private string GenerarClaveTemporal()
        {
            const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
            byte[] data = new byte[10];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(data);

            var sb = new StringBuilder(10);
            foreach (byte b in data)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }

        /// <summary>
        /// Extrae "Primer Nombre Primer Apellido" del campo usu_nombres completo.
        /// Ejemplo: "Juan Carlos Pérez López" → "Juan Pérez"
        /// </summary>
        private string ObtenerPrimerNombreApellido(string nombreCompleto)
        {
            var partes = nombreCompleto.Trim().Split(
                new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length >= 4)
                return $"{partes[0]} {partes[2]}";          // 1er nombre + 1er apellido
            if (partes.Length == 3)
                return $"{partes[0]} {partes[2]}";
            if (partes.Length == 2)
                return $"{partes[0]} {partes[1]}";
            return partes.Length == 1 ? partes[0] : nombreCompleto;
        }

        private bool PareceCorreo(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Contains("@");
        }

        private bool TieneCelularValido(string celular)
        {
            if (string.IsNullOrWhiteSpace(celular))
                return false;

            string soloDigitos = new string(celular.Where(char.IsDigit).ToArray());
            return soloDigitos.Length >= 10;
        }

        /// <summary>Envía SMS vía Twilio con la clave temporal.</summary>
        private void EnviarSmsTwilio(string celular, string nombre, string claveTemporal)
        {
            // Formato ecuatoriano: 09XXXXXXXX → +5939XXXXXXXX
            string soloDigitos = new string(celular.Where(char.IsDigit).ToArray());
            string celularE164 = soloDigitos.StartsWith("0")
                ? "+593" + soloDigitos.Substring(1)
                : "+" + soloDigitos;

            TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);

            MessageResource.Create(
                to: new Twilio.Types.PhoneNumber(celularE164),
                from: new Twilio.Types.PhoneNumber(TwilioNumero),
                body: $"Hola {nombre}, esta es tu clave temporal: {claveTemporal}. " +
                      "Debes cambiarla al ingresar al sistema."
            );
        }

        private void EnviarClaveTemporalPorCorreo(string destinatario, string nombre, string claveTemporal)
        {
            string cuerpo = $@"
<html>
<body style='font-family:Arial,sans-serif;padding:32px;color:#1f2937;'>
  <h2 style='color:#0f3460;'>Recuperacion de contraseña</h2>
  <p>Hola {nombre},</p>
  <p>Tu clave temporal es:</p>
  <p style='font-size:24px;font-weight:bold;letter-spacing:1px;color:#0f3460;'>{claveTemporal}</p>
  <p>Usala para ingresar al sistema y cambia tu contraseña de inmediato.</p>
  <p style='color:#6b7280;font-size:12px;'>Si no solicitaste este cambio, ignora este mensaje.</p>
</body>
</html>";

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(SmtpUsuario, RemitenteNombre);
                mail.To.Add(destinatario);
                mail.Subject = "Tu clave temporal de acceso";
                mail.Body = cuerpo;
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto))
                {
                    smtp.Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        private void EnviarQrPorCorreo(string destinatario, string nombre, string otp)
        {
            string contenidoQr = "OTP:" + otp;
            string urlQr = "https://api.qrserver.com/v1/create-qr-code/" +
                           $"?size=250x250&data={Uri.EscapeDataString(contenidoQr)}";

            string cuerpo = $@"
<html>
<body style='font-family:Arial,sans-serif;text-align:center;padding:40px;'>
  <h2 style='color:#0f3460;'>Hola, {nombre}</h2>
  <p>Escanea el siguiente código QR con la cámara de tu computadora
     para completar el inicio de sesión.</p>
  <p><strong>Valido por 5 minutos.</strong></p>
  <br/>
  <img src='{urlQr}' alt='Codigo QR' style='width:250px;height:250px;'/>
  <br/><br/>
  <p style='color:#999;font-size:12px;'>
    Si no solicitaste este acceso, ignora este mensaje.</p>
</body>
</html>";

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(SmtpUsuario, RemitenteNombre);
                mail.To.Add(destinatario);
                mail.Subject = "Tu codigo QR de acceso";
                mail.Body = cuerpo;
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(SmtpHost, SmtpPuerto))
                {
                    smtp.Credentials = new NetworkCredential(SmtpUsuario, SmtpPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}
