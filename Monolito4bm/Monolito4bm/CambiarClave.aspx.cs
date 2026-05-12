using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class CambiarClave : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo pueden entrar aquí usuarios con clave temporal activa
            if (Session["usu_id"] == null || Session["clave_temporal"] == null
                || !(bool)Session["clave_temporal"])
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void btnCambiar_Click(object sender, EventArgs e)
        {
            string nueva = txtNueva.Text;
            string confirmar = txtConfirmar.Text;

            if (nueva != confirmar)
            {
                MostrarMensaje("Las contraseñas no coinciden.", "error");
                return;
            }

            try
            {
                int usuId = Convert.ToInt32(Session["usu_id"]);
                _svc.CambiarContrasena(usuId, nueva);

                // Limpiar flag temporal de sesión
                Session["clave_temporal"] = false;

                MostrarMensaje("✅ Contraseña actualizada correctamente. Redirigiendo...", "exito");

                // Redirigir al login para que haga el flujo completo (QR)
                Response.AddHeader("REFRESH", "2;URL=Default.aspx");
            }
            catch (ArgumentException ex)
            {
                MostrarMensaje(ex.Message, "error");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error interno: " + ex.Message, "error");
            }
        }

        private void MostrarMensaje(string texto, string tipo)
        {
            divMsg.InnerText = texto;
            divMsg.Attributes["data-tipo"] = tipo;
            pnlMensaje.Visible = true;
        }
    }
}