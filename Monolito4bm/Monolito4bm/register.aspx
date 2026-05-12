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
        .btn-secondary { background-color: #007bff; color: white; padding: 10px 12px; border: none; border-radius: 4px; margin-top: 8px; cursor: pointer; }
        .btn-secondary:hover { background-color: #0069d9; }
        .preview-panel { margin-top: 15px; }
        .preview-grid { display: flex; flex-wrap: wrap; gap: 12px; margin-top: 10px; }
        .preview-card { width: 120px; background: #fafafa; border: 1px solid #ddd; border-radius: 6px; padding: 10px; text-align: center; }
        .preview-card img { width: 100px; height: 100px; object-fit: cover; border-radius: 4px; border: 1px solid #ccc; }
        .preview-name { display: block; margin: 8px 0; font-size: 12px; color: #555; word-break: break-word; }
        .btn-delete { background-color: #dc3545; color: white; border: none; border-radius: 4px; padding: 6px 10px; cursor: pointer; font-size: 12px; }
        .btn-delete:hover { background-color: #c82333; }
        .preview-help { margin-top: 8px; color: #666; font-size: 13px; }
        .input-soft-error { border-color: #dc3545 !important; background-color: #fff8f8; }
        .input-soft-ok { border-color: #28a745 !important; }
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
                <asp:FileUpload ID="fuFotos" runat="server" AllowMultiple="true" onchange="validarFotos(this)" />
                <asp:Button ID="btnPrevisualizar" runat="server" Text="Cargar y previsualizar fotos" OnClick="btnPrevisualizar_Click" formnovalidate="formnovalidate" CssClass="btn-secondary" />

                <asp:Panel ID="pnlPreview" runat="server" CssClass="preview-panel" Visible="false">
                    <asp:Label ID="lblFotosInfo" runat="server" CssClass="preview-help"></asp:Label>
                    <asp:Repeater ID="rptFotosPreview" runat="server" OnItemCommand="rptFotosPreview_ItemCommand">
                        <HeaderTemplate>
                            <div class="preview-grid">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="preview-card">
                                <asp:Image ID="imgPreview" runat="server" ImageUrl='<%# Eval("PreviewUrl") %>' AlternateText='<%# Eval("NombreArchivo") %>' />
                                <span class="preview-name"><%# Eval("NombreArchivo") %></span>
                                <asp:Button ID="btnEliminarFoto" runat="server" Text="Eliminar" CssClass="btn-delete" CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" UseSubmitBehavior="true" />
                            </div>
                        </ItemTemplate>
                        <FooterTemplate>
                            </div>
                        </FooterTemplate>
                    </asp:Repeater>
                </asp:Panel>

            <asp:Button ID="btnRegistrar" runat="server" Text="Finalizar Registro" CssClass="btn" OnClick="btnRegistrar_Click" />
        </div>
    </form>

    <script>
        function setFieldState(input, isValid, message) {
            input.classList.remove('input-soft-error', 'input-soft-ok');
            input.removeAttribute('title');

            if (input.value.trim() === '') {
                return;
            }

            if (isValid) {
                input.classList.add('input-soft-ok');
                return;
            }

            input.classList.add('input-soft-error');
            if (message) {
                input.title = message;
            }
        }

        // 1. Validar Nombres (Min 3 letras)
        function validarNombre(input) {
            let valor = input.value.trim();
            if (valor === '') {
                setFieldState(input, true, '');
                return true;
            }
            let valido = valor.length >= 3;
            setFieldState(input, valido, 'Este campo requiere mínimo 3 letras.');
            return valido;
        }

        // 2. Sugerir Correo automáticamente (N1.A1@dominio.com)
        function sugerirCorreo() {
            let n1 = document.getElementById('<%= txtN1.ClientID %>').value.split(' ')[0];
            let a1 = document.getElementById('<%= txtA1.ClientID %>').value.split(' ')[0];
            let correo = document.getElementById('<%= txtCorreo.ClientID %>');
            if (n1 && a1 && correo.value.trim() === '') {
                correo.value = n1.toLowerCase() + "." + a1.toLowerCase() + "@dominio.com";
                validarCorreo(correo);
            }
        }

        // 3. Validar Cédula Ecuatoriana (Algoritmo JS instantáneo)
        function validarCedula(input) {
            let cedula = input.value.trim();
            if (cedula === '') {
                setFieldState(input, true, '');
                return true;
            }
            if (cedula.length !== 10 || isNaN(cedula)) {
                setFieldState(input, false, 'La cédula debe tener 10 números.');
                return false;
            }
            let prov = parseInt(cedula.substring(0, 2));
            if (prov < 1 || prov > 24 || parseInt(cedula[2]) >= 6) {
                setFieldState(input, false, 'Estructura de cédula inválida.');
                return false;
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
                setFieldState(input, false, 'Número de cédula no válido.');
                return false;
            }
            setFieldState(input, true, '');
            return true;
        }

        // 4. Validar Correo formato
        function validarCorreo(input) {
            let valor = input.value.trim();
            if (valor === '') {
                setFieldState(input, true, '');
                return true;
            }
            let regex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
            let valido = regex.test(valor);
            setFieldState(input, valido, 'Formato de correo inválido.');
            return valido;
        }
        // 5. Validar Password (Norma ISO 27001 Frontend)
        function validarPass(input) {
            let valor = input.value.trim();
            if (valor === '') {
                setFieldState(input, true, '');
                return true;
            }
            let regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$/;
            let valido = regex.test(valor);
            setFieldState(input, valido, 'Use mínimo 8 caracteres con mayúscula, minúscula, número y símbolo.');
            return valido;
        }

        // 6. Validar Edad >= 18 y autocompletar
        function validarEdad(input) {
            if (!input.value) {
                document.getElementById('<%= txtEdad.ClientID %>').value = '';
                setFieldState(input, true, '');
                return true;
            }
            let fechaNac = new Date(input.value);
            let hoy = new Date();
            let edad = hoy.getFullYear() - fechaNac.getFullYear();
            let m = hoy.getMonth() - fechaNac.getMonth();
            if (m < 0 || (m === 0 && hoy.getDate() < fechaNac.getDate())) { edad--; }
            
            document.getElementById('<%= txtEdad.ClientID %>').value = edad;
            
            if (edad < 18 || isNaN(edad)) {
                document.getElementById('<%= txtEdad.ClientID %>').value = '';
                setFieldState(input, false, 'Debe ser mayor de 18 años.');
                return false;
            }
            setFieldState(input, true, '');
            return true;
        }

        // 7. Validar Celular 10 dígitos
        function validarCelular(input) {
            let valor = input.value.trim();
            if (valor === '') {
                setFieldState(input, true, '');
                return true;
            }
            let valido = valor.length === 10 && !isNaN(valor);
            setFieldState(input, valido, 'Celular debe tener 10 números.');
            return valido;
        }

        // 8. Validar Fotos (Cantidad 3-5, Tipo Imagen, Peso máx 2MB)
        function validarFotos(input) {
            let files = input.files;
            if (!files || files.length === 0) {
                input.removeAttribute('title');
                return true;
            }
            let maxPeso = 2 * 1024 * 1024; // 2MB
            for (let i = 0; i < files.length; i++) {
                if (!files[i].type.startsWith('image/')) {
                    input.title = 'El archivo ' + files[i].name + ' no es una imagen válida.';
                    return false;
                }
                if (files[i].size > maxPeso) {
                    input.title = 'La imagen ' + files[i].name + ' supera los 2MB.';
                    return false;
                }
            }
            input.removeAttribute('title');
            return true;
        }
    </script>
</body>
</html>
