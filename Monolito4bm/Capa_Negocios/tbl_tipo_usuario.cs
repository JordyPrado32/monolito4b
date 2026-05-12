using System;
using System.Collections.Generic;

namespace Capa_Negocios
{
    // ENTIDADES
    public class tbl_tipo_usuario
    {
        public int rol_id { get; set; }
        public string nombre_rol { get; set; }
    }

    // LÓGICA DE NEGOCIO
    public class UsuarioLogica
    {
        // 1. MÉTODO PARA LLENAR EL DDL DE PERFILES
        public List<tbl_tipo_usuario> ObtenerRolesParaDDL()
        {
            // AQUÍ llamas a tu Capa de Datos para traer de SQL. 
            // Como ejemplo, devuelvo una lista quemada para que veas cómo funciona el DDL:
            return new List<tbl_tipo_usuario>
            {
                new tbl_tipo_usuario { rol_id = 1, nombre_rol = "Administrador" },
                new tbl_tipo_usuario { rol_id = 2, nombre_rol = "Usuario Estándar" }
            };
        }

        // 2. MÉTODO PRINCIPAL DE REGISTRO
        public void ProcesarRegistroUsuario(tbl_usuario usuario, string contrasenaSinEncriptar)
        {
            // Validaciones (asumo que mantienes las que hicimos antes: cédula, edad, nombres)
            if (contrasenaSinEncriptar.Length < 3) throw new Exception("La contraseña es muy corta.");

            // Encriptar y asignar automáticos
            usuario.contraseña = BCrypt.Net.BCrypt.HashPassword(contrasenaSinEncriptar);
            usuario.fecha_creacion = DateTime.Now;
            // usuario.usu_nickname = GenerarNickname(...) // (Tu lógica de nickname aquí)

            // === EL PUENTE A LA BASE DE DATOS ===
            // Aquí es donde mandas el objeto a la Capa de Datos para el INSERT.
            // Capa_Datos.UsuarioDatos.InsertarUsuario(usuario);
        }

        // 3. MÉTODO ESPECÍFICO PARA REGISTRAR FOTOS (Como lo solicitaste)
        public void RegistrarFotosUsuario(int idUsuarioGenerado, List<tbl_usuario_fotos> listaFotos)
        {
            if (listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe seleccionar entre 3 y 5 fotos.");

            bool esPrimera = true;
            foreach (var foto in listaFotos)
            {
                foto.usu_id = idUsuarioGenerado; // Asignamos a quién pertenece la foto
                foto.fecha_subida = DateTime.Now;
                foto.es_principal = esPrimera;
                esPrimera = false;

                // === EL PUENTE A LA BASE DE DATOS PARA FOTOS ===
                // Capa_Datos.FotoDatos.InsertarFoto(foto);
            }
        }
    }
}