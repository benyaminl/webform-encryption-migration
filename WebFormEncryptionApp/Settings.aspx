<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WebFormEncryptionApp.SettingsPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Settings - File Encryption App</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" />
</head>
<body>
    <form id="formSettings" runat="server">
    <div class="container mt-4">
        <h1>Settings</h1>
        <p><a href="Default.aspx">&larr; Back to Main</a></p>

        <!-- Normal user: clear own data -->
        <div class="card mt-3">
            <div class="card-body">
                <h5 class="card-title">Clear My Data</h5>
                <p class="card-text">Delete your encryption history and files.</p>
                <asp:Button ID="btnClearOwn" runat="server" Text="Clear My History & Files"
                    CssClass="btn btn-warning" OnClick="BtnClearOwn_Click"
                    OnClientClick="return confirm('Delete all YOUR history and files?');" />
            </div>
        </div>

        <!-- Admin-only section -->
        <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
            <div class="card mt-3">
                <div class="card-body">
                    <h5 class="card-title">Admin: Clear User Data</h5>
                    <div class="mb-3">
                        <label class="form-label">Select user:</label>
                        <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-select" />
                    </div>
                    <asp:Button ID="btnClearUser" runat="server" Text="Clear Selected User's Data"
                        CssClass="btn btn-danger" OnClick="BtnClearUser_Click"
                        OnClientClick="return confirm('Delete selected user history and files?');" />
                    <asp:Button ID="btnClearAll" runat="server" Text="Clear ALL Data"
                        CssClass="btn btn-danger ms-2" OnClick="BtnClearAll_Click"
                        OnClientClick="return confirm('Delete ALL history and files for ALL users?');" />
                </div>
            </div>
        </asp:Panel>

        <asp:Label ID="lblStatus" runat="server" CssClass="d-block mt-3 fw-bold" />
    </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
