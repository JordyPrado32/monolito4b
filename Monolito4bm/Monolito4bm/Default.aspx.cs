using System;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string nick = txtNick.Text.Trim();
            string pass = txtPass.Text;

            int rolId;
            string correo, msgError;

            ResultadoLogin resultado = _svc.IniciarSesion(nick, pass,
                                           out rolId, out correo, out msgError);

            switch (resultado)
            {
                case ResultadoLogin.Exitoso:
                    Session["correo_qr"] = correo;
                    Session["rol_id"] = rolId;
                    Response.Redirect("~/VerificarQR.aspx");
                    break;

                case ResultadoLogin.ExitosoClaveTemporalActiva:
                    // Guardar usu_id en sesión para que CambiarClave pueda usarlo
                    // (necesitas que ObtenerUsuarioPorNick también devuelva usu_id,
                    //  ya lo hace — lo guardamos aquí vía Session)
                    Session["usu_id"] = ObtenerUsuIdDeNick(nick);
                    Session["clave_temporal"] = true;
                    Response.Redirect("~/CambiarClave.aspx");
                    break;

                case ResultadoLogin.UsuarioBloqueado:
                case ResultadoLogin.CredencialesInvalidas:
                case ResultadoLogin.ErrorInterno:
                    MostrarError(msgError);
                    txtPass.Text = string.Empty;
                    break;
            }
        }
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            Response.Redirect("register.aspx");
        }

        protected void btnRecuperar_Click(object sender, EventArgs e)
        {
            Response.Redirect("RecuperarClave.aspx");
        }

        /// <summary>
        /// Obtiene el usu_id usando la capa de datos directamente.
        /// Se hace aquí para no cambiar la firma de IniciarSesion.
        /// </summary>
        private int ObtenerUsuIdDeNick(string nick)
        {
            var gestor = new Capa_Datos.GestorDatos();
            var dt = gestor.ObtenerUsuarioPorNick(nick);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["usu_id"]) : 0;
        }

        private void MostrarError(string msg)
        {
            // Esto forzará una ventana emergente en el navegador
            ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('ERROR: {msg}');", true);
            litError.Text = msg;
            pnlError.Visible = true;
        }
    }
}

        
