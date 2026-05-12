<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="register.aspx.cs" Inherits="Monolito4bm.register" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Registro Estricto 3 Capas</title>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        body { font-family: sans-serif; background-color: #f4f4f4; display: flex; justify-content: center; padding: 20px; }
        .container { background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); width: 450px; }
        h2 { text-align: center; color: #333; }
        label { display: block; margin-top: 10px; font-weight: bold; color: #555; }
        input, select { width: 100%; padding: 10px; margin-top: 5px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
        input[readonly] { background-color: #eee; }
        .row { display: flex; gap: 10px; }
        .col { flex: 1; }
        .btn { background-color: #28a745; color: white; padding: 12px; border: none; border-radius: 4px; width: 100%; margin-top: 20px; cursor: pointer; font-size: 16px; }
        .btn:hover { background-color: #218838; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Registro de Usuario</h2>

            <div class="row">
                <div class="col">
                    <label>Primer Nombre:</label>
                    <asp:TextBox ID="txtN1" runat="server" onblur="validarNombre(this)"></asp:TextBox>
                </div>
                <div class="col">
                    <label>Segundo Nombre:</label>
                    <asp:TextBox ID="txtN2" runat="server" onblur="validarNombre(this)"></asp:TextBox>
                </div>
            </div>
            <div class="row">
                <div class="col">
                    <label>Primer Apellido:</label>
                    <asp:TextBox ID="txtA1" runat="server" onblur="validarNombre(this); sugerirCorreo();"></asp:TextBox>
                </div>
                <div class="col">
                    <label>Segundo Apellido:</label>
                    <asp:TextBox ID="txtA2" runat="server" onblur="validarNombre(this)"></asp:TextBox>
                </div>
            </div>
            <label>Nickname (Autogenerado por el sistema):</label>
<asp:TextBox ID="txtNickname" runat="server" ReadOnly="true" placeholder="Aparecerá aquí al registrarse..." BackColor="#e9ecef"></asp:TextBox>

            <label>Cédula Ecuatoriana:</label>
            <asp:TextBox ID="txtCedula" runat="server" MaxLength="10" onblur="validarCedula(this)"></asp:TextBox>

            <label>Correo (@dominio.com):</label>
            <asp:TextBox ID="txtCorreo" runat="server" onblur="validarCorreo(this)"></asp:TextBox>

            <label>Contraseña :</label>
            <asp:TextBox ID="txtPass" runat="server" TextMode="Password" onblur="validarPass(this)"></asp:TextBox>

            <div class="row">
                <div class="col">
                    <label>Fecha Nacimiento:</label>
                    <asp:TextBox ID="txtFechaNac" runat="server" TextMode="Date" onblur="validarEdad(this)"></asp:TextBox>
                </div>
                <div class="col">
                    <label>Edad:</label>
                    <asp:TextBox ID="txtEdad" runat="server" ReadOnly="true"></asp:TextBox>
                </div>
            </div>

            <label>Celular (10 dígitos):</label>
            <asp:TextBox ID="txtCelular" runat="server" MaxLength="10" onblur="validarCelular(this)"></asp:TextBox>

            <label>Perfil (Roles BD):</label>
            <asp:DropDownList ID="ddlTipoUsuario" runat="server"></asp:DropDownList>
            
            <label>Fotos (Sube 3 a 5):</label>
                <asp:FileUpload ID="fuFotos" runat="server" AllowMultiple="true" />

                <asp:Button ID="btnPrevisualizar" runat="server" Text="Vista Previa de Foto 1" OnClick="btnPrevisualizar_Click" formnovalidate="formnovalidate" style="margin-top:5px;" />
                <br />
                <asp:Image ID="imgPreview" runat="server" Width="150px" Visible="false" style="margin-top:10px; border: 1px solid #ccc; border-radius: 5px;" />

            <asp:Button ID="btnRegistrar" runat="server" Text="Finalizar Registro" CssClass="btn" OnClick="btnRegistrar_Click" />
        </div>
    </form>

    <script>
        // 1. Validar Nombres (Min 3 letras)
        function validarNombre(input) {
            if (input.value.trim().length < 3) {
                Swal.fire('Atención', 'Este campo requiere mínimo 3 letras.', 'warning');
                input.value = '';
            }
        }

        // 2. Sugerir Correo automáticamente (N1.A1@dominio.com)
        function sugerirCorreo() {
            let n1 = document.getElementById('<%= txtN1.ClientID %>').value.split(' ')[0];
            let a1 = document.getElementById('<%= txtA1.ClientID %>').value.split(' ')[0];
            let correo = document.getElementById('<%= txtCorreo.ClientID %>');
            if (n1 && a1 && correo.value == '') {
                correo.value = n1.toLowerCase() + "." + a1.toLowerCase() + "@dominio.com";
            }
        }

        // 3. Validar Cédula Ecuatoriana (Algoritmo JS instantáneo)
        function validarCedula(input) {
            let cedula = input.value;
            if (cedula.length !== 10 || isNaN(cedula)) {
                Swal.fire('Error', 'La cédula debe tener 10 números.', 'error');
                input.value = ''; return;
            }
            let prov = parseInt(cedula.substring(0, 2));
            if (prov < 1 || prov > 24 || parseInt(cedula[2]) >= 6) {
                Swal.fire('Error', 'Estructura de cédula inválida.', 'error');
                input.value = ''; return;
            }
            let coef = [2, 1, 2, 1, 2, 1, 2, 1, 2];
            let suma = 0;
            for (let i = 0; i < 9; i++) {
                let val = parseInt(cedula[i]) * coef[i];
                suma += (val > 9) ? val - 9 : val;
            }
            let verificador = parseInt(cedula[9]);
            let dsuperior = (Math.ceil(suma / 10) * 10);
            let calculado = dsuperior - suma;
            if (calculado === 10) calculado = 0;
            if (calculado !== verificador) {
                Swal.fire('Error', 'Número de cédula no existe.', 'error');
                input.value = '';
            }
        }

        // 4. Validar Correo formato
        function validarCorreo(input) {
            let regex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
            if (!regex.test(input.value)) {
                Swal.fire('Error', 'Formato de correo inválido.', 'error');
                input.value = '';
            }
        }
        // 5. Validar Password (Norma ISO 27001 Frontend)
        function validarPass(input) {
            // Regex: Mínimo 8, 1 Mayus, 1 Minus, 1 Número, 1 Especial
            let regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$/;
            if (!regex.test(input.value) && input.value !== '') {
                Swal.fire('Seguridad', 'La contraseña debe tener mínimo 8 caracteres e incluir mayúsculas, minúsculas, números y un símbolo especial.', 'warning');
                input.value = '';
            }
        }
        // 6. Validar Edad >= 18 y autocompletar
        function validarEdad(input) {
            let fechaNac = new Date(input.value);
            let hoy = new Date();
            let edad = hoy.getFullYear() - fechaNac.getFullYear();
            let m = hoy.getMonth() - fechaNac.getMonth();
            if (m < 0 || (m === 0 && hoy.getDate() < fechaNac.getDate())) { edad--; }
            
            document.getElementById('<%= txtEdad.ClientID %>').value = edad;
            
            if (edad < 18 || isNaN(edad)) {
                Swal.fire('Prohibido', 'Debe ser mayor de 18 años.', 'error');
                input.value = '';
                document.getElementById('<%= txtEdad.ClientID %>').value = '';
            }
        }

        // 7. Validar Celular 10 dígitos
        function validarCelular(input) {
            if (input.value.length !== 10 || isNaN(input.value)) {
                Swal.fire('Error', 'Celular debe tener 10 números.', 'error');
                input.value = '';
            }
        }

        // 8. Validar Fotos (Cantidad 3-5, Tipo Imagen, Peso máx 2MB)
        function validarFotos(input) {
            let files = input.files;
            if (files.length < 3 || files.length > 5) {
                Swal.fire('Atención', 'Debe seleccionar entre 3 y 5 archivos.', 'warning');
                input.value = ''; return;
            }
            let maxPeso = 2 * 1024 * 1024; // 2MB
            for (let i = 0; i < files.length; i++) {
                if (!files[i].type.startsWith('image/')) {
                    Swal.fire('Error', 'El archivo ' + files[i].name + ' no es una imagen.', 'error');
                    input.value = ''; return;
                }
                if (files[i].size > maxPeso) {
                    Swal.fire('Error', 'La imagen ' + files[i].name + ' supera los 2MB.', 'error');
                    input.value = ''; return;
                }
            }
        }
    </script>
</body>
</html>