using Capa_Datos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios
{
    public class tbl_usuario
    {
        public int usu_id { get; set; }
        public string usu_nombres { get; set; }
        public string usu_apellidos { get; set; }
        public string usu_cedula { get; set; }
        public string correo_electronico { get; set; }
        public string contraseña { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string usu_nickname { get; set; }
        public string numero_celular { get; set; }
        public int rol_id { get; set; }
        public DateTime fecha_creacion { get; set; }

        public List<tbl_usuario_fotos> Fotos { get; set; } = new List<tbl_usuario_fotos>();

        public int Registrarse(string passSinEncriptar, string nombresSeparados, string apellidosSeparados)
        {
            // 1. Encriptar
            this.contraseña = BCrypt.Net.BCrypt.HashPassword(passSinEncriptar);

            // 2. Fechas
            this.fecha_creacion = DateTime.Now;

            // 3. Generar Nickname
            this.usu_nickname = GenerarNickname(nombresSeparados, apellidosSeparados, this.usu_cedula);

            // 4. Concatenar para la base
            this.usu_nombres = nombresSeparados.Trim() + " " + apellidosSeparados.Trim();

            // 5. LLAMAR A DATOS (Le pasamos las propiedades sueltas)
            GestorDatos bd = new GestorDatos();
            return bd.InsertarUsuarioDatos(
                this.usu_nombres, this.usu_cedula, this.correo_electronico,
                this.contraseña, this.fecha_nacimiento, this.edad,
                this.usu_nickname, this.numero_celular, this.rol_id, this.fecha_creacion
            );
        }

        // METODO DEL NICKNAME AQUÍ MISMO
        private string GenerarNickname(string nombres, string apellidos, string cedula)
        {
            var n = nombres.Trim().Split(' ');
            var a = apellidos.Trim().Split(' ');

            char c1 = n[0][0];
            char c2 = a[0][0];
            char c3 = a.Length > 1 && a[1].Length > 1 ? a[1][1] : a[0][1];
            char c4 = n.Length > 1 && n[1].Length > 1 ? n[1][1] : n[0][1];

            string letras = $"{c1}{c2}{c3}{c4}";
            Random rnd = new Random();
            var mezcladas = letras.Select(c => rnd.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c)).ToArray();

            string simbolos = "@#$*&";
            char sim = simbolos[rnd.Next(simbolos.Length)];
            char num1 = cedula.Length > 0 ? cedula[rnd.Next(cedula.Length)] : '0';
            char num2 = cedula.Length > 0 ? cedula[rnd.Next(cedula.Length)] : '1';

            return $"{sim}{new string(mezcladas)}{num1}{num2}";
        }
    }

    public class tbl_usuario_fotos
    {
        public int foto_id { get; set; }
        public int usu_id { get; set; }
        public string nombre_archivo { get; set; }
        public string content_type { get; set; }
        public byte[] foto { get; set; } // Si guardas el archivo en BD, o string si es ruta
        public DateTime fecha_subida { get; set; }
        public bool es_principal { get; set; }
    }
}