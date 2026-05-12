using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class VerificarQR : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Proteger: si no hay sesión activa, redirigir al login
            if (Session["correo_qr"] == null)
                Response.Redirect("~/Default.aspx");
        }

        protected void btnValidar_Click(object sender, EventArgs e)
        {
            string otp = hdnOtp.Value.Trim();

            if (string.IsNullOrEmpty(otp))
                return;

            int rolId = _svc.ValidarOtp(otp);

            switch (rolId)
            {
                case 1:
                    Session["autenticado"] = true;
                    Response.Redirect("~/PaginaRol1.aspx");
                    break;

                case 2:
                    Session["autenticado"] = true;
                    Response.Redirect("~/PaginaRol2.aspx");
                    break;

                default:
                    // OTP inválido o expirado — redirigir al login limpio
                    Session.Clear();
                    Response.Redirect("~/Default.aspx?error=qr_invalido");
                    break;
            }
        }
    }
}