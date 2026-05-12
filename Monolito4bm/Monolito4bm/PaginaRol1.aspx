<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaginaRol1.aspx.cs" Inherits="Monolito4bm.PaginaRol1" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Panel Administrador</title>
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
            color: #1f2937;
        }
        .page {
            max-width: 1200px;
            margin: 0 auto;
            padding: 32px 20px 48px;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 16px;
            margin-bottom: 24px;
        }
        .title h1 {
            margin: 0 0 8px;
            color: #0f172a;
            font-size: 2rem;
        }
        .title p {
            margin: 0;
            color: #475569;
        }
        .btn {
            display: inline-block;
            padding: 10px 14px;
            border-radius: 10px;
            border: none;
            cursor: pointer;
            font-weight: 700;
            text-decoration: none;
            transition: .2s ease;
        }
        .btn-primary {
            background: #0f3460;
            color: #fff;
        }
        .btn-primary:hover { background: #16213e; }
        .btn-secondary {
            background: #fff;
            color: #0f3460;
            border: 1px solid #0f3460;
        }
        .btn-secondary:hover { background: #eff6ff; }
        .card {
            background: #fff;
            border-radius: 18px;
            padding: 24px;
            box-shadow: 0 18px 40px rgba(15, 23, 42, 0.12);
        }
        .msg {
            border-radius: 10px;
            padding: 12px 14px;
            margin-bottom: 18px;
            font-size: .95rem;
        }
        .msg.ok {
            background: #ecfdf5;
            color: #166534;
            border: 1px solid #86efac;
        }
        .msg.error {
            background: #fef2f2;
            color: #b91c1c;
            border: 1px solid #fca5a5;
        }
        .table-wrap {
            overflow-x: auto;
        }
        .grid {
            width: 100%;
            border-collapse: collapse;
            min-width: 980px;
        }
        .grid th, .grid td {
            padding: 14px 12px;
            border-bottom: 1px solid #e5e7eb;
            text-align: left;
            vertical-align: middle;
        }
        .grid th {
            background: #eff6ff;
            color: #1e3a8a;
            font-size: .9rem;
        }
        .badge {
            display: inline-block;
            padding: 6px 10px;
            border-radius: 999px;
            font-size: .8rem;
            font-weight: 700;
        }
        .badge.ok {
            background: #dcfce7;
            color: #166534;
        }
        .badge.warn {
            background: #fee2e2;
            color: #991b1b;
        }
        .actions {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
        }
        .inline-btn {
            padding: 8px 10px;
            border-radius: 8px;
            border: none;
            cursor: pointer;
            font-size: .85rem;
            font-weight: 700;
        }
        .inline-reset {
            background: #e0f2fe;
            color: #075985;
        }
        .inline-unlock {
            background: #dcfce7;
            color: #166534;
        }
        .empty {
            padding: 24px 0 8px;
            color: #64748b;
        }
        @media (max-width: 720px) {
            .header {
                flex-direction: column;
                align-items: stretch;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="page">
            <div class="header">
                <div class="title">
                    <h1>Panel de administrador</h1>
                    <p>Consulta todas las cuentas, revisa intentos fallidos y desbloquea usuarios con clave temporal.</p>
                </div>
                <asp:Button ID="btnCerrarSesion" runat="server" Text="Cerrar sesión" CssClass="btn btn-secondary" OnClick="btnCerrarSesion_Click" CausesValidation="false" />
            </div>

            <div class="card">
                <asp:Panel ID="pnlMensaje" runat="server" Visible="false">
                    <asp:Literal ID="litMensaje" runat="server"></asp:Literal>
                </asp:Panel>

                <div style="margin-bottom:16px;">
                    <asp:Button ID="btnRecargar" runat="server" Text="Actualizar listado" CssClass="btn btn-primary" OnClick="btnRecargar_Click" CausesValidation="false" />
                </div>

                <div class="table-wrap">
                    <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="grid"
                        GridLines="None" OnRowCommand="gvUsuarios_RowCommand" EmptyDataText="No se encontraron cuentas registradas.">
                        <Columns>
                            <asp:BoundField DataField="usu_id" HeaderText="ID" />
                            <asp:BoundField DataField="usu_nombres" HeaderText="Nombre completo" />
                            <asp:BoundField DataField="usu_nickname" HeaderText="Usuario" />
                            <asp:BoundField DataField="correo_electronico" HeaderText="Correo" />
                            <asp:BoundField DataField="rol_id" HeaderText="Rol" />
                            <asp:BoundField DataField="intentos_fallidos" HeaderText="Intentos" />
                            <asp:BoundField DataField="ultimo_intento" HeaderText="Último intento" DataFormatString="{0:dd/MM/yyyy HH:mm}" HtmlEncode="false" />
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <span class='badge <%# EsCuentaBloqueada(Eval("estado_cuenta")) ? "warn" : "ok" %>'>
                                        <%# Eval("estado_cuenta") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <div class="actions">
                                        <asp:Button ID="btnResetear" runat="server" Text="Resetear intentos"
                                            CssClass="inline-btn inline-reset"
                                            CommandName="ResetearIntentos"
                                            CommandArgument='<%# Eval("usu_id") %>'
                                            CausesValidation="false" />
                                        <asp:Button ID="btnDesbloquear" runat="server" Text="Desbloquear y enviar clave"
                                            CssClass="inline-btn inline-unlock"
                                            CommandName="DesbloquearCuenta"
                                            CommandArgument='<%# Eval("usu_id") %>'
                                            CausesValidation="false"
                                            Visible='<%# EsCuentaBloqueada(Eval("estado_cuenta")) %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
