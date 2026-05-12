using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class register : System.Web.UI.Page
    {
        private const string SessionFotosKey = "RegisterFotosPreview";
        private const int MinFotos = 3;
        private const int MaxFotos = 5;
        private const int MaxPesoFotoBytes = 2 * 1024 * 1024;

        [Serializable]
        private class FotoTemporal
        {
            public string Id { get; set; }
            public string NombreArchivo { get; set; }
            public string ContentType { get; set; }
            public byte[] Contenido { get; set; }

            public string PreviewUrl
            {
                get { return "data:" + ContentType + ";base64," + Convert.ToBase64String(Contenido); }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRolesDropdown();
                LimpiarFotosTemporales();
            }

            BindFotosPreview();
        }

        private void CargarRolesDropdown()
        {
            try
            {
                tbl_tipo_usuario negocioRol = new tbl_tipo_usuario();
                ddlTipoUsuario.DataSource = negocioRol.CargarRoles();
                ddlTipoUsuario.DataTextField = "nombre_rol";
                ddlTipoUsuario.DataValueField = "rol_id";
                ddlTipoUsuario.DataBind();
                ddlTipoUsuario.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Seleccione Perfil --", "0"));
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("error", "Error crítico", ex.Message);
            }
        }

        protected void btnPrevisualizar_Click(object sender, EventArgs e)
        {
            try
            {
                GuardarFotosTemporalesDesdeUpload();
                BindFotosPreview();
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("warning", "Atención", ex.Message);
            }
        }

        protected void rptFotosPreview_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Eliminar")
            {
                return;
            }

            List<FotoTemporal> fotos = ObtenerFotosTemporales();
            FotoTemporal fotoEliminar = fotos.FirstOrDefault(f => f.Id == e.CommandArgument.ToString());

            if (fotoEliminar == null)
            {
                InjectarSweetAlert("warning", "Atención", "La imagen seleccionada ya no está disponible.");
                BindFotosPreview();
                return;
            }

            fotos.Remove(fotoEliminar);
            GuardarFotosTemporales(fotos);
            BindFotosPreview();
            InjectarSweetAlert("success", "Imagen eliminada", "La foto fue retirada de la previsualización.");
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlTipoUsuario.SelectedValue == "0")
                {
                    throw new Exception("Seleccione un perfil.");
                }

                List<FotoTemporal> fotosTemporales = ObtenerFotosTemporales();
                if (fotosTemporales.Count == 0 && fuFotos.HasFiles)
                {
                    GuardarFotosTemporalesDesdeUpload();
                    fotosTemporales = ObtenerFotosTemporales();
                }

                if (fotosTemporales.Count < MinFotos || fotosTemporales.Count > MaxFotos)
                {
                    throw new Exception("Debe mantener entre 3 y 5 fotografías antes de finalizar el registro.");
                }

                tbl_usuario nuevoUsuario = new tbl_usuario();
                nuevoUsuario.usu_cedula = txtCedula.Text;
                nuevoUsuario.correo_electronico = txtCorreo.Text;
                nuevoUsuario.numero_celular = txtCelular.Text;
                nuevoUsuario.rol_id = int.Parse(ddlTipoUsuario.SelectedValue);

                DateTime fechaNacimiento;
                if (!DateTime.TryParse(txtFechaNac.Text, out fechaNacimiento))
                {
                    throw new Exception("Fecha de nacimiento no válida.");
                }

                nuevoUsuario.fecha_nacimiento = fechaNacimiento;

                int calculoEdad = DateTime.Today.Year - fechaNacimiento.Year;
                if (fechaNacimiento.Date > DateTime.Today.AddYears(-calculoEdad))
                {
                    calculoEdad--;
                }

                nuevoUsuario.edad = calculoEdad;

                int idUsuarioGenerado = nuevoUsuario.Registrarse(
                    txtPass.Text, txtN1.Text, txtN2.Text, txtA1.Text, txtA2.Text
                );

                List<tbl_foto> listaFotos = new List<tbl_foto>();
                foreach (FotoTemporal fotoTemporal in fotosTemporales)
                {
                    listaFotos.Add(new tbl_foto
                    {
                        nombre_archivo = fotoTemporal.NombreArchivo,
                        content_type = fotoTemporal.ContentType,
                        foto = fotoTemporal.Contenido
                    });
                }

                tbl_foto negocioFotos = new tbl_foto();
                negocioFotos.RegistrarFotosValidadas(idUsuarioGenerado, listaFotos);

                txtNickname.Text = nuevoUsuario.usu_nickname;

                LimpiarFotosTemporales();
                BindFotosPreview();

                InjectarSweetAlert("success", "Registro exitoso", $"Usuario creado.<br>Tu Nickname es: <b>{nuevoUsuario.usu_nickname}</b>");
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("error", "Error en registro", ex.Message);
            }
        }

        private void GuardarFotosTemporalesDesdeUpload()
        {
            if (!fuFotos.HasFiles)
            {
                throw new Exception("Seleccione al menos una imagen para agregar a la previsualización.");
            }

            List<FotoTemporal> fotosActuales = ObtenerFotosTemporales();
            int capacidadDisponible = MaxFotos - fotosActuales.Count;

            if (capacidadDisponible <= 0)
            {
                throw new Exception("Ya alcanzó el máximo de 5 imágenes.");
            }

            if (fuFotos.PostedFiles.Count > capacidadDisponible)
            {
                throw new Exception("Solo puede agregar " + capacidadDisponible + " imagen(es) más.");
            }

            List<FotoTemporal> nuevasFotos = new List<FotoTemporal>();

            foreach (var file in fuFotos.PostedFiles)
            {
                if (file == null || file.ContentLength == 0)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("El archivo " + file.FileName + " no es una imagen válida.");
                }

                if (file.ContentLength > MaxPesoFotoBytes)
                {
                    throw new Exception("La imagen " + file.FileName + " supera los 2MB permitidos.");
                }

                using (BinaryReader br = new BinaryReader(file.InputStream))
                {
                    nuevasFotos.Add(new FotoTemporal
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        NombreArchivo = Path.GetFileName(file.FileName),
                        ContentType = file.ContentType,
                        Contenido = br.ReadBytes(file.ContentLength)
                    });
                }
            }

            if (nuevasFotos.Count == 0)
            {
                throw new Exception("No se encontraron imágenes válidas para cargar.");
            }

            fotosActuales.AddRange(nuevasFotos);
            GuardarFotosTemporales(fotosActuales);
        }

        private List<FotoTemporal> ObtenerFotosTemporales()
        {
            return Session[SessionFotosKey] as List<FotoTemporal> ?? new List<FotoTemporal>();
        }

        private void GuardarFotosTemporales(List<FotoTemporal> fotos)
        {
            Session[SessionFotosKey] = fotos;
        }

        private void LimpiarFotosTemporales()
        {
            Session.Remove(SessionFotosKey);
        }

        private void BindFotosPreview()
        {
            List<FotoTemporal> fotos = ObtenerFotosTemporales();
            pnlPreview.Visible = fotos.Count > 0;
            rptFotosPreview.DataSource = fotos;
            rptFotosPreview.DataBind();

            if (fotos.Count > 0)
            {
                string estado = fotos.Count >= MinFotos
                    ? "Ya puede finalizar el registro."
                    : "Necesita al menos " + (MinFotos - fotos.Count) + " imagen(es) más para registrar.";
                lblFotosInfo.Text = "Imágenes cargadas: " + fotos.Count + " de " + MaxFotos + ". " + estado;
            }
            else
            {
                lblFotosInfo.Text = string.Empty;
            }
        }

        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string script = $@"<script>Swal.fire({{ icon: '{tipo}', title: '{titulo}', html: '{mensaje.Replace("'", "\\'")}' }});</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "Swal", script);
        }
    }
}
