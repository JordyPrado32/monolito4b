<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="register.aspx.cs" Inherits="Monolito4bm.register" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Registro de Usuario</title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="width: 400px; margin: auto; font-family: Arial;">
            <h2>Registro de Usuario</h2>

            <label>Nombres (Min. 2):</label>
            <asp:TextBox ID="txtNombres" runat="server" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Apellidos (Min. 2):</label>
            <asp:TextBox ID="txtApellidos" runat="server" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Cédula:</label>
            <asp:TextBox ID="txtCedula" runat="server" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Correo Electrónico:</label>
            <asp:TextBox ID="txtCorreo" runat="server" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Contraseña:</label>
            <asp:TextBox ID="txtContrasena" runat="server" TextMode="Password" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Fecha de Nacimiento:</label>
            <asp:TextBox ID="txtFechaNac" runat="server" TextMode="Date" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Celular:</label>
            <asp:TextBox ID="txtCelular" runat="server" Width="100%"></asp:TextBox>
            <br /><br />

            <label>Perfil (Tipo de Usuario):</label>
            <asp:DropDownList ID="ddlTipoUsuario" runat="server" Width="100%"></asp:DropDownList>
            <br /><br />

            <label>Fotos (Sube de 3 a 5 imágenes):</label>
            <asp:FileUpload ID="fuFotos" runat="server" AllowMultiple="true" Width="100%" />
            <br /><br />

            <asp:Button ID="btnRegistrar" runat="server" Text="Registrar Usuario" OnClick="btnRegistrar_Click" />
            <br /><br />
            
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" Font-Bold="true"></asp:Label>
        </div>
    </form>
</body>
</html>