using System;
using System.Collections.Generic;
using Capa_Datos; // Ve a Datos

namespace Capa_Negocios
{
    public class tbl_foto
    {
        public int foto_id { get; set; }
        public int usu_id { get; set; }
        public string nombre_archivo { get; set; }
        public string content_type { get; set; }
        public byte[] foto { get; set; }
        public DateTime fecha_subida { get; set; }
        public bool es_principal { get; set; }

        // Método para procesar y guardar la lista de fotos validada
        public void RegistrarFotosValidadas(int usuarioId, List<tbl_foto> listaFotos)
        {
            // Validación Backend de cantidad (3 a 5)
            if (listaFotos.Count < 3 || listaFotos.Count > 5)
                throw new Exception("Debe subir entre 3 y 5 fotografías válidas.");

            GestorDatos bd = new GestorDatos();
            bool primera = true;

            foreach (var f in listaFotos)
            {
                bd.InsertarFotoDatos(
                    usuarioId,
                    f.nombre_archivo,
                    f.content_type,
                    f.foto,
                    DateTime.Now, // Fecha subida actual
                    primera // La primera es principal
                );
                primera = false;
            }
        }
    }
}