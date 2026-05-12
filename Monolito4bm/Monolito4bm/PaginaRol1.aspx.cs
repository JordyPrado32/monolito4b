using System;
using System.Data;
using Capa_Negocios;

namespace Monolito4bm
{
    public partial class PaginaRol1 : System.Web.UI.Page
    {
        private readonly UsuarioService _svc = new UsuarioService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!EsAdminAutenticado())
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack)
                CargarUsuarios();
        }

        protected void btnRecargar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
            MostrarMensaje("Listado actualizado correctamente.", false);
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        protected void gvUsuarios_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int usuId))
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "ResetearIntentos":
                        _svc.ResetearIntentosUsuario(usuId);
                        MostrarMensaje("Los intentos del usuario fueron reiniciados.", false);
                        break;

                    case "DesbloquearCuenta":
                        string correoDestino;
                        string claveTemporal;
                        _svc.DesbloquearCuentaConClaveTemporal(usuId, out correoDestino, out claveTemporal);
                        MostrarMensaje("La cuenta fue desbloqueada y se envió una clave temporal a " + correoDestino + ".", false);
                        break;
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje(ex.Message, true);
            }

            CargarUsuarios();
        }

        protected bool EsCuentaBloqueada(object estado)
        {
            return Convert.ToString(estado).IndexOf("bloque", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CargarUsuarios()
        {
            DataTable dt = _svc.ObtenerEstadoCuentas();
            gvUsuarios.DataSource = dt;
            gvUsuarios.DataBind();
        }

        private bool EsAdminAutenticado()
        {
            return Session["autenticado"] != null
                   && Convert.ToBoolean(Session["autenticado"])
                   && Session["rol_id"] != null
                   && Convert.ToInt32(Session["rol_id"]) == 1;
        }

        private void MostrarMensaje(string mensaje, bool esError)
        {
            pnlMensaje.CssClass = esError ? "msg error" : "msg ok";
            litMensaje.Text = Server.HtmlEncode(mensaje);
            pnlMensaje.Visible = true;
        }
    }
}
