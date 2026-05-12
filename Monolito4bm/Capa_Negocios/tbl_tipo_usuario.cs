using System;
using System.Collections.Generic;
using System.Data;
using Capa_Datos; // Ve a Datos

namespace Capa_Negocios
{
    public class tbl_tipo_usuario
    {
        public int rol_id { get; set; }
        public string nombre_rol { get; set; }

        // Método para traer roles reales
        public List<tbl_tipo_usuario> CargarRoles()
        {
            List<tbl_tipo_usuario> lista = new List<tbl_tipo_usuario>();
            GestorDatos datos = new GestorDatos();
            DataTable dt = datos.ObtenerRolesDatos();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new tbl_tipo_usuario
                {
                    rol_id = Convert.ToInt32(row["rol_id"]),
                    nombre_rol = row["nombre_rol"].ToString()
                });
            }
            return lista;
        }
    }
}