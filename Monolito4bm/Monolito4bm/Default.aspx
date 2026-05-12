<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Monolito4bm.Default" %>
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8"/>
  <meta name="viewport" content="width=device-width, initial-scale=1"/>
  <title>Iniciar Sesión</title>
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
      max-width: 400px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.4);
    }
    .card h2 {
      text-align: center;
      margin-bottom: 28px;
      color: #0f3460;
      font-size: 1.6rem;
    }
    .field { margin-bottom: 18px; }
    .field label {
      display: block;
      margin-bottom: 6px;
      color: #444;
      font-size: .88rem;
      font-weight: 600;
    }
    .field input[type=text],
    .field input[type=password] {
      width: 100%;
      padding: 10px 14px;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: .95rem;
      transition: border-color .2s;
      outline: none;
    }
    .field input:focus { border-color: #0f3460; }
    .btn-login {
      width: 100%;
      padding: 12px;
      background: #0f3460;
      color: #fff;
      border: none;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      margin-top: 8px;
      transition: background .2s;
    }
    .btn-login:hover { background: #16213e; }
    .msg-error {
      background: #fff0f0;
      color: #c0392b;
      border: 1px solid #e74c3c;
      border-radius: 8px;
      padding: 10px 14px;
      margin-bottom: 18px;
      font-size: .88rem;
      display: none;
    }
    .msg-error.visible { display: block; }
  </style>
</head>
<body>
  <form id="frmLogin" runat="server">
    <div class="card">
      <h2>🔐 Iniciar Sesión</h2>

      <!-- Panel de error (server-side) -->
      <asp:Panel ID="pnlError" runat="server" CssClass="msg-error" Visible="false">
        <asp:Literal ID="litError" runat="server"/>
      </asp:Panel>

      <div class="field">
        <label for="txtNick">Nombre de usuario</label>
        <asp:TextBox ID="txtNick" runat="server" CssClass="" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfvNick" runat="server"
          ControlToValidate="txtNick" ErrorMessage="Requerido"
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <div class="field">
        <label for="txtPass">Contraseña</label>
        <asp:TextBox ID="txtPass" runat="server" TextMode="Password" ClientIDMode="Static"/>
        <asp:RequiredFieldValidator ID="rfvPass" runat="server"
          ControlToValidate="txtPass" ErrorMessage="Requerido"
          Display="Dynamic" ForeColor="Red" Font-Size="Small"/>
      </div>

      <asp:Button ID="btnLogin" runat="server" Text="Entrar"
        CssClass="btn-login" OnClick="btnLogin_Click"/>
        <asp:Button ID="btnRegister" runat="server" Text="Crear cuenta nueva"
        CssClass="btn-register" OnClick="btnRegister_Click" CausesValidation="false" />
    </div>
  </form>
</body>
</html>
