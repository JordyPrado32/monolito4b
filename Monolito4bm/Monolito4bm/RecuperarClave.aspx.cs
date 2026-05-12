using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class RecuperarClave : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            string valor = txtNickOCorreo.Text.Trim();
            string mensajeErr;

            ResultadoRecuperacion resultado = _svc.RecuperarClave(valor, out mensajeErr);

            switch (resultado)
            {
                case ResultadoRecuperacion.Exitoso:
                    MostrarMensaje(
                        "✅ Se envió una clave temporal a tu número registrado. Úsala para ingresar y cámbiala de inmediato.",
                        "exito");
                    txtNickOCorreo.Text = string.Empty;
                    btnEnviar.Enabled = false;
                    break;

                case ResultadoRecuperacion.UsuarioNoEncontrado:
                case ResultadoRecuperacion.ErrorInterno:
                    MostrarMensaje(mensajeErr, "error");
                    break;
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