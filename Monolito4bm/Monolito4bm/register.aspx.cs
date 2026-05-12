using System;
using System.IO;
using Capa_Negocios; // SOLO NEGOCIOS. Cero contacto con Datos.

namespace Monolito4bm
{
    public partial class register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Carga tu DDL manualmente o llamando a un método en tbl_rol
                ddlTipoUsuario.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Seleccione un Perfil --", "0"));
                ddlTipoUsuario.Items.Add(new System.Web.UI.WebControls.ListItem("Usuario Estándar", "2"));
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlTipoUsuario.SelectedValue == "0") throw new Exception("Seleccione un Perfil.");

                // 1. Instanciar tu clase
                tbl_usuario nuevoUser = new tbl_usuario();

                // 2. Llenar propiedades con los textboxes
                nuevoUser.usu_cedula = txtCedula.Text;
                nuevoUser.correo_electronico = txtCorreo.Text;
                nuevoUser.fecha_nacimiento = DateTime.Parse(txtFechaNac.Text);
                nuevoUser.numero_celular = txtCelular.Text;
                nuevoUser.rol_id = int.Parse(ddlTipoUsuario.SelectedValue);

                // 3. Calcular edad
                int calculoEdad = DateTime.Today.Year - nuevoUser.fecha_nacimiento.Year;
                if (nuevoUser.fecha_nacimiento.Date > DateTime.Today.AddYears(-calculoEdad)) calculoEdad--;
                nuevoUser.edad = calculoEdad;

                // 4. LLAMAR AL MÉTODO QUE ESTÁ DENTRO DE LA CLASE tbl_usuario
                int idCreado = nuevoUser.Registrarse(txtContrasena.Text, txtNombres.Text, txtApellidos.Text);

                // 5. Tema Fotos (Puedes hacer lo mismo de poner el método dentro de tbl_fotos)
                if (fuFotos.HasFiles)
                {
                    // Lógica para recorrer y guardar fotos usando tu clase tbl_foto...
                }

                MostrarAlerta("success", "Éxito", "Usuario registrado. Nickname: " + nuevoUser.usu_nickname);
            }
            catch (Exception ex)
            {
                MostrarAlerta("error", "Error", ex.Message);
            }
        }

        private void MostrarAlerta(string tipo, string titulo, string mensaje)
        {
            string script = $@"<script>Swal.fire('{titulo}', '{mensaje.Replace("'", "\\'")}', '{tipo}');</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "SweetAlert", script);
        }
    }
}