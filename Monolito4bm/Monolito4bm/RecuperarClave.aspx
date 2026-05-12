<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarClave.aspx.cs" Inherits="Monolito4bm.RecuperarClave" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Recuperar Contraseña</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }
    .card {
      background: #fff;
      border-radius: 16px;
      padding: 40px 36px;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.4);
    }
    .card h2 { text-align:center; color:#0f3460; margin-bottom:8px; font-size:1.5rem; }
    .card .subtitulo {
      text-align:center; color:#777; font-size:.88rem;
      margin-bottom:28px; line-height:1.5;
    }
    .field { margin-bottom: 18px; }
    .field label {
      display:block; margin-bottom:6px;
      color:#444; font-size:.88rem; font-weight:600;
    }
    .field input[type=text] {
      width:100%; padding:10px 14px;
      border:1px solid #ccc; border-radius:8px;
      font-size:.95rem; outline:none; transition:border-color .2s;
    }
    .field input:focus { border-color:#0f3460; }
    .btn-recuperar {
      width:100%; padding:12px;
      background:#0f3460; color:#fff;
      border:none; border-radius:8px;
      font-size:1rem; font-weight:700;
      cursor:pointer; margin-top:8px;
      transition:background .2s;
    }
    .btn-recuperar:hover { background:#16213e; }
    .volver {
      display:block; text-align:center;
      margin-top:16px; color:#0f3460;
      font-size:.88rem; text-decoration:none;
    }
    .volver:hover { text-decoration:underline; }
    .msg {
      border-radius:8px; padding:10px 14px;
      margin-bottom:18px; font-size:.88rem; display:none;
    }
    .msg.visible  { display:block; }
    .msg.error    { background:#fff0f0; color:#c0392b; border:1px solid #e74c3c; }
    .msg.exito    { background:#e6f9ef; color:#1a7a3d; border:1px solid #27ae60; }
  </style>
</head>
<body>
  <form id="frmRecuperar" runat="server">
    <div class="card">
      <h2>🔑 Recuperar Contraseña</h2>
      <p class="subtitulo">Ingresa tu nickname o correo electrónico y recibirás<br/>una clave temporal por SMS.</p>

      <asp:Panel ID="pnlMensaje" runat="server" Visible="false">
        <div id="divMsg" class="msg" runat="server"></div>
      </asp:Panel>

      <div class="field">
        <label>Nickname o correo electrónico</label>
        <asp:TextBox ID="txtNickOCorreo" runat="server" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfv" runat="server"
          ControlToValidate="txtNickOCorreo" ErrorMessage="Campo requerido."
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <asp:Button ID="btnEnviar" runat="server" Text="Enviar clave temporal"
                  CssClass="btn-recuperar" OnClick="btnEnviar_Click"/>

      <a href="Default.aspx" class="volver">← Volver al login</a>
    </div>
  </form>

  <script>
    // Aplica clase CSS al div del mensaje según el atributo data-tipo
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
