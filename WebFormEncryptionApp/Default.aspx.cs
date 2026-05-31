using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace WebFormEncryptionApp
{
    public partial class DefaultPage : Page
    {
        protected global::System.Web.UI.WebControls.FileUpload fuSourceFile;
        protected global::System.Web.UI.WebControls.TextBox txtOutputName;
        protected global::System.Web.UI.WebControls.TextBox txtPassword;
        protected global::System.Web.UI.WebControls.Button btnEncrypt;
        protected global::System.Web.UI.WebControls.Button btnDecrypt;
        protected global::System.Web.UI.WebControls.Label lblStatus;
        protected global::System.Web.UI.WebControls.Label lblUsername;
        protected global::System.Web.UI.WebControls.HyperLink lnkUsers;
        protected global::System.Web.UI.WebControls.LinkButton lnkLogout;
        protected global::System.Web.UI.WebControls.GridView gvHistory;
        protected global::System.Web.UI.WebControls.Button btnRefresh;

        private int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                // Restore session from auth cookie
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var user = Global.AuthService.ValidateLogin(HttpContext.Current.User.Identity.Name, "null");
                    // Can't restore without password; force re-login
                    FormsAuthentication.SignOut();
                    Response.Redirect("Login.aspx");
                    return;
                }
                Response.Redirect("Login.aspx");
                return;
            }

            lblUsername.Text = HttpUtility.HtmlEncode(Session["Username"].ToString());
            if (Session["IsAdmin"] != null && (bool)Session["IsAdmin"])
                lnkUsers.Visible = true;
            if (!IsPostBack)
            {
                BindHistory();
            }
        }

        protected void LnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            Response.Redirect("Login.aspx");
        }

        protected void BtnEncrypt_Click(object sender, EventArgs e)
        {
            ProcessFile("Encrypt", "encrypted_");
        }

        protected void BtnDecrypt_Click(object sender, EventArgs e)
        {
            ProcessFile("Decrypt", "decrypted_");
        }

        protected void BtnRefresh_Click(object sender, EventArgs e)
        {
            BindHistory();
        }

        private void ProcessFile(string action, string prefix)
        {
            try
            {
                if (!fuSourceFile.HasFile)
                {
                    ShowStatus("Please select a source file.", Color.Red);
                    return;
                }

                var result = Global.ValidationService.ValidateInputs(
                    fuSourceFile.FileName, txtPassword.Text, "output");
                if (!result.IsValid)
                {
                    ShowStatus(result.ErrorMessage, Color.Red);
                    return;
                }

                var originalName = fuSourceFile.FileName;
                var sourceName = GenerateTimestampedName(originalName, "");
                var sourcePath = Path.Combine(Global.FilesPath, sourceName);
                fuSourceFile.SaveAs(sourcePath);

                var outputOriginal = string.IsNullOrWhiteSpace(txtOutputName.Text)
                    ? originalName : txtOutputName.Text;
                var outputName = GenerateTimestampedName(outputOriginal, prefix);
                var outputPath = Path.Combine(Global.FilesPath, outputName);

                if (action == "Encrypt")
                    Global.EncryptionService.EncryptFile(sourcePath, outputPath, txtPassword.Text);
                else
                    Global.DecryptionService.DecryptFile(sourcePath, outputPath, txtPassword.Text);

                Global.HistoryService.SaveToHistory(originalName, sourceName, outputName, action, CurrentUserId);
                BindHistory();
                ShowStatus("File " + action.ToLower() + "ed successfully!", Color.Green);
            }
            catch (Exception ex)
            {
                ShowStatus(action + " failed: " + ex.Message, Color.Red);
            }
        }

        private void BindHistory()
        {
            gvHistory.DataSource = Global.HistoryService.LoadHistory(CurrentUserId);
            gvHistory.DataBind();
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private static string GenerateTimestampedName(string originalFilename, string prefix)
        {
            var now = DateTime.Now;
            var offset = TimeZoneInfo.Local.GetUtcOffset(now);
            var sign = offset.Hours >= 0 ? "+" : "";
            var stamp = now.ToString("yyyy-MM-dd_HHmmss") + "_GMT" + sign + offset.Hours;
            var name = Path.GetFileNameWithoutExtension(originalFilename);
            var ext = Path.GetExtension(originalFilename);
            return prefix + name + "_" + stamp + ext;
        }
    }
}
