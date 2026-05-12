using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Capa_Negocios
{
    public class UsuarioService
    {
        // Método principal de registro
        public void RegistrarUsuario(tbl_usuario nuevoUsuario, string nombresSeparados, string apellidosSeparados, List<tbl_usuario_fotos> fotosAdjuntas)
        {
            // 1. Validar Nombres y Apellidos (Mínimo 2 nombres de 3 letras, igual apellidos)
            var arrNombres = nombresSeparados.Trim().Split(' ');
            var arrApellidos = apellidosSeparados.Trim().Split(' ');
            if (arrNombres.Length < 2 || arrNombres.Any(n => n.Length < 3) ||
                arrApellidos.Length < 2 || arrApellidos.Any(a => a.Length < 3))
            {
                throw new ArgumentException("Debe ingresar dos nombres y dos apellidos de mínimo 3 letras cada uno.");
            }
            nuevoUsuario.usu_nombres = $"{nombresSeparados} {apellidosSeparados}"; // Concatenado para la BD según tu diagrama

            // 2. Validar Cédula Ecuatoriana
            if (!ValidarCedulaEcuatoriana(nuevoUsuario.usu_cedula))
                throw new ArgumentException("Cédula ecuatoriana inválida.");

            // 3. Validar Celular (10 dígitos numéricos)
            if (nuevoUsuario.numero_celular.Length != 10 || !nuevoUsuario.numero_celular.All(char.IsDigit))
                throw new ArgumentException("El número de celular debe tener exactamente 10 dígitos.");

            // 4. Validar Edad >= 18 y autocalcular
            int edadCalculada = CalcularEdad(nuevoUsuario.fecha_nacimiento);
            if (edadCalculada < 18)
                throw new ArgumentException("El usuario debe ser mayor de 18 años.");
            nuevoUsuario.edad = edadCalculada;

            // 5. Validar Correo y Contraseña
            if (!Regex.IsMatch(nuevoUsuario.correo_electronico, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("El correo electrónico no tiene un formato válido (@dominio.com).");

            if (nuevoUsuario.contraseña.Length < 3)
                throw new ArgumentException("La contraseña debe tener mínimo 3 caracteres.");

            // 6. Encriptar contraseña usando BCrypt
            nuevoUsuario.contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.contraseña);

            // 7. Autogenerar Nickname
            nuevoUsuario.usu_nickname = GenerarNickname(arrNombres, arrApellidos, nuevoUsuario.usu_cedula);

            // 8. Carga de datos automáticos y Rol
            nuevoUsuario.fecha_creacion = DateTime.Now;
            nuevoUsuario.rol_id = 2; // Ejemplo: 2 = Rol "Usuario Estándar"

            // 9. Validar e insertar Fotos
            if (fotosAdjuntas.Count < 3 || fotosAdjuntas.Count > 5)
                throw new ArgumentException("Debe subir un mínimo de 3 y un máximo de 5 fotografías.");

            bool primera = true;
            foreach (var foto in fotosAdjuntas)
            {
                foto.fecha_subida = DateTime.Now;
                foto.es_principal = primera; // La primera foto se marca como principal
                primera = false;
                nuevoUsuario.Fotos.Add(foto);
            }

            // AQUÍ LLAMAS A TU CAPA DE DATOS PARA GUARDAR EL OBJETO 'nuevoUsuario'
            // CapaDatos.InsertarUsuario(nuevoUsuario);
        }

        // --- MÉTODOS AUXILIARES (LÓGICA PURA) ---

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            int edad = DateTime.Today.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;
            return edad;
        }

        private string GenerarNickname(string[] n, string[] a, string cedula)
        {
            char c1 = n[0][0]; // 1ra letra primer nombre
            char c2 = a[0][0]; // 1ra letra primer apellido
            char c3 = a[1].Length > 1 ? a[1][1] : a[1][0]; // 2da letra segundo apellido
            char c4 = n[1].Length > 1 ? n[1][1] : n[1][0]; // 2da letra segundo nombre

            string letras = $"{c1}{c2}{c3}{c4}";
            Random rnd = new Random();

            // Mayúsculas y minúsculas al azar
            var letrasMezcladas = letras.Select(c => rnd.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c)).ToArray();

            string simbolos = "@#$*&";
            char simbolo = simbolos[rnd.Next(simbolos.Length)];
            char num1 = cedula[rnd.Next(cedula.Length)];
            char num2 = cedula[rnd.Next(cedula.Length)];

            return $"{simbolo}{new string(letrasMezcladas)}{num1}{num2}";
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit)) return false;
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
    }
}