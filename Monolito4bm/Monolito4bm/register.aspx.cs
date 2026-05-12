using System;
using System.Collections.Generic;
using System.IO;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRolesDropdown();
            }
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
                InjectarSweetAlert("error", "Error Crítico", ex.Message);
            }
        }

        // --- NUEVO: PREVISUALIZAR FOTO EN C# PURA ---
        protected void btnPrevisualizar_Click(object sender, EventArgs e)
        {
            if (fuFotos.HasFile)
            {
                // Leemos solo el primer archivo para la vista previa
                using (BinaryReader br = new BinaryReader(fuFotos.PostedFile.InputStream))
                {
                    byte[] bytes = br.ReadBytes(fuFotos.PostedFile.ContentLength);
                    string base64String = Convert.ToBase64String(bytes);

                    imgPreview.ImageUrl = "data:image/png;base64," + base64String;
                    imgPreview.Visible = true;
                }
            }
            else
            {
                InjectarSweetAlert("warning", "Atención", "Seleccione al menos una foto primero.");
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlTipoUsuario.SelectedValue == "0") throw new Exception("Seleccione un Perfil.");
                if (!fuFotos.HasFiles) throw new Exception("Debe subir las fotografías.");

                tbl_usuario nuevoUsuario = new tbl_usuario();
                nuevoUsuario.usu_cedula = txtCedula.Text;
                nuevoUsuario.correo_electronico = txtCorreo.Text;
                nuevoUsuario.numero_celular = txtCelular.Text;
                nuevoUsuario.rol_id = int.Parse(ddlTipoUsuario.SelectedValue);

                // SOLUCIÓN AL ERROR DE FORMATO: Validar fecha y calcular edad en C#
                DateTime fechaNacimiento;
                if (!DateTime.TryParse(txtFechaNac.Text, out fechaNacimiento))
                    throw new Exception("Fecha de nacimiento no válida.");

                nuevoUsuario.fecha_nacimiento = fechaNacimiento;

                int calculoEdad = DateTime.Today.Year - fechaNacimiento.Year;
                if (fechaNacimiento.Date > DateTime.Today.AddYears(-calculoEdad)) calculoEdad--;
                nuevoUsuario.edad = calculoEdad;

                // Llamar al registro (Aquí se valida ISO 27001 y se crea el Nickname)
                int idUsuarioGenerado = nuevoUsuario.Registrarse(
                    txtPass.Text, txtN1.Text, txtN2.Text, txtA1.Text, txtA2.Text
                );

                // Procesar Fotos
                List<tbl_foto> listaFotos = new List<tbl_foto>();
                foreach (var file in fuFotos.PostedFiles)
                {
                    using (BinaryReader br = new BinaryReader(file.InputStream))
                    {
                        listaFotos.Add(new tbl_foto
                        {
                            nombre_archivo = file.FileName,
                            content_type = file.ContentType,
                            foto = br.ReadBytes(file.ContentLength)
                        });
                    }
                }

                tbl_foto negocioFotos = new tbl_foto();
                negocioFotos.RegistrarFotosValidadas(idUsuarioGenerado, listaFotos);
                // Mostrar el nickname en el textbox visual
                txtNickname.Text = nuevoUsuario.usu_nickname;

                InjectarSweetAlert("success", "¡Registro Exitoso!", $"Usuario creado.<br>Tu Nickname es: <b>{nuevoUsuario.usu_nickname}</b>");
            }
            catch (Exception ex)
            {
                InjectarSweetAlert("error", "Error en Registro", ex.Message);
            }
        }

        private void InjectarSweetAlert(string tipo, string titulo, string mensaje)
        {
            string script = $@"<script>Swal.fire({{ icon: '{tipo}', title: '{titulo}', html: '{mensaje.Replace("'", "\\'")}' }});</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "Swal", script);
        }
    }
}