using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace WebFormEncryptionApp
{
    public partial class LoginPage : Page
    {
        protected global::System.Web.UI.WebControls.TextBox txtUsername;
        protected global::System.Web.UI.WebControls.TextBox txtPassword;
        protected global::System.Web.UI.WebControls.Button btnLogin;
        protected global::System.Web.UI.WebControls.Label lblError;

        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = Global.AuthService.ValidateLogin(txtUsername.Text.Trim(), txtPassword.Text);
            if (user == null)
            {
                lblError.Text = "Invalid username or password.";
                return;
            }

            FormsAuthentication.SetAuthCookie(user.Username, false);
            Session["UserId"] = user.Id;
            Session["Username"] = user.Username;
            Session["IsAdmin"] = user.IsAdmin;

            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
                Response.Redirect(returnUrl);
            else
                Response.Redirect("Default.aspx");
        }
    }
}
