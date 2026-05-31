<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebFormEncryptionApp.LoginPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login - File Encryption App</title>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; background: #f5f5f5; }
        .login-box { background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); width: 320px; }
        .login-box h1 { margin: 0 0 20px; font-size: 1.4em; text-align: center; }
        .field { margin-bottom: 12px; }
        .field label { display: block; font-weight: bold; margin-bottom: 4px; }
        .field input { width: 100%; padding: 8px; box-sizing: border-box; }
        .btn { width: 100%; padding: 10px; background: #0078d4; color: #fff; border: none; cursor: pointer; font-size: 1em; }
        .btn:hover { background: #005a9e; }
        .error { color: red; margin-top: 10px; text-align: center; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-box">
            <h1>File Encryption App</h1>
            <div class="field">
                <label>Username:</label>
                <asp:TextBox ID="txtUsername" runat="server" />
            </div>
            <div class="field">
                <label>Password:</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
            </div>
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn" OnClick="BtnLogin_Click" />
            <asp:Label ID="lblError" runat="server" CssClass="error" />
        </div>
    </form>
</body>
</html>
