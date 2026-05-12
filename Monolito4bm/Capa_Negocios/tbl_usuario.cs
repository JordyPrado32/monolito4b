using System;
using System.Linq;
using System.Text.RegularExpressions;
using Capa_Datos; // Ve a Datos

namespace Capa_Negocios
{
    public class tbl_usuario
    {
        // Propiedades básicas
        public int usu_id { get; set; }
        public string usu_nombres { get; set; }
        public string usu_cedula { get; set; }
        public string correo_electronico { get; set; }
        public string contraseña { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string usu_nickname { get; set; }
        public string numero_celular { get; set; }
        public int rol_id { get; set; }
        public DateTime fecha_creacion { get; set; }

        public int Registrarse(string passSinEncriptar, string n1, string n2, string a1, string a2)
        {
            // NUEVO: Validación de Contraseña Norma ISO 27001
            // Mínimo 8, 1 mayúscula, 1 minúscula, 1 número, 1 caracter especial
            var regexPass = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$");
            if (!regexPass.IsMatch(passSinEncriptar))
            {
                throw new Exception("La contraseña no cumple la ISO 27001.<br>Debe tener mín 8 caracteres, incluir mayúsculas, minúsculas, números y caracteres especiales.");
            }
            // 1. VALIDACIONES BACKEND (Seguridad extra)
            ValidarNombres(n1, n2, a1, a2);
            if (!ValidarCedulaEcuatoriana(this.usu_cedula)) throw new Exception("Cédula inválida.");
            if (!Regex.IsMatch(this.correo_electronico, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new Exception("Correo inválido.");
            if (passSinEncriptar.Length < 3) throw new Exception("Contraseña mínimo 3 caracteres.");
            if (this.edad < 18) throw new Exception("Debe ser mayor de 18 años.");
            if (this.numero_celular.Length != 10 || !this.numero_celular.All(char.IsDigit)) throw new Exception("Celular debe tener 10 dígitos.");

            // 2. PROCESAMIENTO
            this.contraseña = BCrypt.Net.BCrypt.HashPassword(passSinEncriptar); // Encriptar
            this.fecha_creacion = DateTime.Now; // Fecha actual
            this.usu_nombres = $"{n1} {n2} {a1} {a2}"; // Concatenar full
            this.usu_nickname = GenerarNickname(n1, a1, a2, n2, this.usu_cedula); // Generar Nick exacto

            // 3. LLAMAR A DATOS
            GestorDatos bd = new GestorDatos();
            return bd.InsertarUsuarioDatos(
                this.usu_nombres, this.usu_cedula, this.correo_electronico,
                this.contraseña, this.fecha_nacimiento,
                this.usu_nickname, this.numero_celular, this.rol_id, this.fecha_creacion
            );
        }

        // --- LÓGICA PRIVADA ---

        private void ValidarNombres(string n1, string n2, string a1, string a2)
        {
            if (string.IsNullOrWhiteSpace(n1) || string.IsNullOrWhiteSpace(n2) || string.IsNullOrWhiteSpace(a1) || string.IsNullOrWhiteSpace(a2))
                throw new Exception("Todos los nombres y apellidos son obligatorios.");
            if (n1.Length < 3 || n2.Length < 3 || a1.Length < 3 || a2.Length < 3)
                throw new Exception("Nombres y apellidos mínimo 3 letras.");
        }

        // Lógica Nickname exacta: Simbolo + 1letra N1 + 1letra A1 + 2da letra A2 + 2da letra N2 + random Case + 2nums Cedula
        private string GenerarNickname(string n1, string a1, string a2, string n2, string cedula)
        {
            Random rnd = new Random();
            string simbolos = "@#$*&";
            char sim = simbolos[rnd.Next(simbolos.Length)];

            // Asegurar capturar letras seguras
            char c1 = n1.Trim()[0];
            char c2 = a1.Trim()[0];
            char c3 = a2.Trim().Length > 1 ? a2.Trim()[1] : a2.Trim()[0];
            char c4 = n2.Trim().Length > 1 ? n2.Trim()[1] : n2.Trim()[0];

            string letras = $"{c1}{c2}{c3}{c4}";
            // Mezclar Mayúsculas/Minúsculas al azar
            var mezcladas = letras.Select(c => rnd.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c)).ToArray();

            // 2 números al azar de la cédula
            char num1 = cedula[rnd.Next(cedula.Length)];
            char num2 = cedula[rnd.Next(cedula.Length)];

            return $"{sim}{new string(mezcladas)}{num1}{num2}";
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit)) return false;
            int prov = int.Parse(cedula.Substring(0, 2));
            if (prov < 1 || prov > 24) return false;
            int tercer = int.Parse(cedula.Substring(2, 1));
            if (tercer >= 6) return false;
            int[] coef = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int val = int.Parse(cedula[i].ToString()) * coef[i];
                suma += val > 9 ? val - 9 : val;
            }
            int verificador = int.Parse(cedula[9].ToString());
            int decenaSuperior = ((suma + 9) / 10) * 10;
            int calculado = decenaSuperior - suma;
            if (calculado == 10) calculado = 0;
            return calculado == verificador;
        }
    }
}