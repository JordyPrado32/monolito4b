<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CambiarClave.aspx.cs" Inherits="Monolito4bm.CambiarClave" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Cambiar Contraseña</title>
  <style>
    * { box-sizing:border-box; margin:0; padding:0; }
    body {
      min-height:100vh; display:flex; align-items:center; justify-content:center;
      background:linear-gradient(135deg,#1a1a2e 0%,#16213e 50%,#0f3460 100%);
      font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;
    }
    .card {
      background:#fff; border-radius:16px; padding:40px 36px;
      width:100%; max-width:420px; box-shadow:0 20px 60px rgba(0,0,0,.4);
    }
    .card h2 { text-align:center; color:#0f3460; margin-bottom:8px; font-size:1.5rem; }
    .subtitulo {
      text-align:center; color:#777; font-size:.88rem;
      margin-bottom:28px; line-height:1.5;
    }
    .field { margin-bottom:18px; }
    .field label { display:block; margin-bottom:6px; color:#444; font-size:.88rem; font-weight:600; }
    .field input[type=password] {
      width:100%; padding:10px 14px; border:1px solid #ccc;
      border-radius:8px; font-size:.95rem; outline:none; transition:border-color .2s;
    }
    .field input:focus { border-color:#0f3460; }
    .hint { font-size:.78rem; color:#999; margin-top:4px; }
    .btn-cambiar {
      width:100%; padding:12px; background:#0f3460; color:#fff;
      border:none; border-radius:8px; font-size:1rem; font-weight:700;
      cursor:pointer; margin-top:8px; transition:background .2s;
    }
    .btn-cambiar:hover { background:#16213e; }
    .msg { border-radius:8px; padding:10px 14px; margin-bottom:18px; font-size:.88rem; display:none; }
    .msg.visible { display:block; }
    .msg.error   { background:#fff0f0; color:#c0392b; border:1px solid #e74c3c; }
    .msg.exito   { background:#e6f9ef; color:#1a7a3d; border:1px solid #27ae60; }
  </style>
</head>
<body>
  <form id="frmCambiar" runat="server">
    <div class="card">
      <h2>🔒 Cambiar Contraseña</h2>
      <p class="subtitulo">Ingresaste con una clave temporal.<br/>Debes establecer una nueva contraseña para continuar.</p>

      <asp:Panel ID="pnlMensaje" runat="server" Visible="false">
        <div id="divMsg" class="msg" runat="server"></div>
      </asp:Panel>

      <div class="field">
        <label>Nueva contraseña</label>
        <asp:TextBox ID="txtNueva" runat="server" TextMode="Password" ClientIDMode="Static"/>
        <p class="hint">Mínimo 3 caracteres.</p>
        <asp:RequiredFieldValidator ID="rfvNueva" runat="server"
          ControlToValidate="txtNueva" ErrorMessage="Campo requerido."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <div class="field">
        <label>Confirmar nueva contraseña</label>
        <asp:TextBox ID="txtConfirmar" runat="server" TextMode="Password" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfvConfirmar" runat="server"
          ControlToValidate="txtConfirmar" ErrorMessage="Campo requerido."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
        <asp:CompareValidator ID="cvClave" runat="server"
          ControlToValidate="txtConfirmar" ControlToCompare="txtNueva"
          ErrorMessage="Las contraseñas no coinciden."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <asp:Button ID="btnCambiar" runat="server" Text="Cambiar contraseña"
                  CssClass="btn-cambiar" OnClick="btnCambiar_Click"/>
    </div>
  </form>

  <script>
    window.onload = function () {
      var d = document.getElementById('divMsg');
      if (d && d.innerText.trim() !== '') {
        d.classList.add('visible');
        d.classList.add(d.getAttribute('data-tipo') || 'error');
      }
    };
  </script>
</body>
</html>
