<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebFormEncryptionApp.DefaultPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>File Encryption App</title>
    <style>
        body { font-family: Segoe UI, Arial, sans-serif; margin: 20px; }
        .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
        .header .user-info { font-size: 0.9em; }
        .field { margin-bottom: 10px; }
        .field label { display: block; font-weight: bold; margin-bottom: 4px; }
        .buttons { margin: 10px 0; }
        .buttons input { margin-right: 10px; }
        .status { margin: 10px 0; font-weight: bold; }
        .history { margin-top: 20px; }
        .history table { border-collapse: collapse; width: 100%; }
        .history th, .history td { border: 1px solid #ccc; padding: 6px 10px; text-align: left; }
        .history th { background: #f0f0f0; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header">
            <h1>File Encryption App</h1>
            <div class="user-info">
                Logged in as: <strong><asp:Label ID="lblUsername" runat="server" /></strong>
                | <a href="Settings.aspx">Settings</a>
                | <asp:HyperLink ID="lnkUsers" runat="server" NavigateUrl="/Users" Text="Users" Visible="false" />
                <asp:LinkButton ID="lnkLogout" runat="server" OnClick="LnkLogout_Click">Logout</asp:LinkButton>
            </div>
        </div>

        <div class="field">
            <label>Source File:</label>
            <asp:FileUpload ID="fuSourceFile" runat="server" />
        </div>

        <div class="field">
            <label>Output Filename (optional):</label>
            <asp:TextBox ID="txtOutputName" runat="server" Width="400px" />
        </div>

        <div class="field">
            <label>Password:</label>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" Width="300px" />
        </div>

        <div class="buttons">
            <asp:Button ID="btnEncrypt" runat="server" Text="Encrypt" OnClick="BtnEncrypt_Click" />
            <asp:Button ID="btnDecrypt" runat="server" Text="Decrypt" OnClick="BtnDecrypt_Click" />
        </div>

        <div class="status">
            <asp:Label ID="lblStatus" runat="server" />
        </div>

        <div class="history">
            <h2>Encryption History</h2>
            <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="False"
                CellPadding="4" GridLines="Both" EmptyDataText="No history yet.">
                <Columns>
                    <asp:BoundField DataField="Filename" HeaderText="Filename" />
                    <asp:BoundField DataField="MachineName" HeaderText="Machine Name" />
                    <asp:BoundField DataField="DateTime" HeaderText="Date Time" />
                    <asp:BoundField DataField="Action" HeaderText="Action" />
                    <asp:TemplateField HeaderText="Source">
                        <ItemTemplate>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# "Download.aspx?file=" + HttpUtility.UrlEncode(Eval("SourcePath").ToString()) %>'
                                Text="Download" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Output">
                        <ItemTemplate>
                            <asp:HyperLink runat="server"
                                NavigateUrl='<%# "Download.aspx?file=" + HttpUtility.UrlEncode(Eval("OutputPath").ToString()) %>'
                                Text="Download" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <br />
            <asp:Button ID="btnRefresh" runat="server" Text="Refresh History" OnClick="BtnRefresh_Click" />
        </div>
    </form>
</body>
</html>
