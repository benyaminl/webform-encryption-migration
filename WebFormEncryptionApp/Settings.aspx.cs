using System;
using System.IO;
using System.Web.UI;
using WebFormEncryptionApp.Services;

namespace WebFormEncryptionApp
{
    public partial class SettingsPage : Page
    {
        protected global::System.Web.UI.WebControls.Button btnClearOwn;
        protected global::System.Web.UI.WebControls.Panel pnlAdmin;
        protected global::System.Web.UI.WebControls.DropDownList ddlUsers;
        protected global::System.Web.UI.WebControls.Button btnClearUser;
        protected global::System.Web.UI.WebControls.Button btnClearAll;
        protected global::System.Web.UI.WebControls.Label lblStatus;

        private bool IsAdmin
        {
            get { return Session["IsAdmin"] != null && (bool)Session["IsAdmin"]; }
        }

        private int CurrentUserId
        {
            get { return Session["UserId"] != null ? (int)Session["UserId"] : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            pnlAdmin.Visible = IsAdmin;
            if (!IsPostBack && IsAdmin)
            {
                BindUserDropdown();
            }
        }

        protected void BtnClearOwn_Click(object sender, EventArgs e)
        {
            DeleteUserFiles(CurrentUserId);
            Global.HistoryService.ClearHistory(CurrentUserId);
            ShowStatus("Your history and files have been deleted.", System.Drawing.Color.Green);
        }

        protected void BtnClearUser_Click(object sender, EventArgs e)
        {
            if (!IsAdmin) return;
            var userId = int.Parse(ddlUsers.SelectedValue);
            DeleteUserFiles(userId);
            Global.HistoryService.ClearHistory(userId);
            ShowStatus("User data cleared.", System.Drawing.Color.Green);
        }

        protected void BtnClearAll_Click(object sender, EventArgs e)
        {
            if (!IsAdmin) return;
            var files = Directory.GetFiles(Global.FilesPath);
            foreach (var file in files) File.Delete(file);
            Global.HistoryService.ClearAllHistory();
            ShowStatus("All history and files deleted.", System.Drawing.Color.Green);
        }

        private void BindUserDropdown()
        {
            ddlUsers.Items.Clear();
            var users = Global.AuthService.GetAllUsers();
            foreach (var u in users)
            {
                ddlUsers.Items.Add(new System.Web.UI.WebControls.ListItem(
                    u.Username + (u.IsAdmin ? " (admin)" : ""), u.Id.ToString()));
            }
        }

        private void DeleteUserFiles(int userId)
        {
            var files = ((HistoryService)Global.HistoryService).GetFilesByUser(userId);
            foreach (var f in files)
            {
                var path = Path.Combine(Global.FilesPath, f);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private void ShowStatus(string message, System.Drawing.Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
