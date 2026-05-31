using System;
using System.IO;
using System.Web;
using System.Web.UI;
using WebFormEncryptionApp.Services;

namespace WebFormEncryptionApp
{
    public partial class DownloadPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            var filename = Request.QueryString["file"];
            if (string.IsNullOrEmpty(filename)
                || filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            // Ownership check: user can only download their own files (admin can download all)
            var isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
            if (!isAdmin)
            {
                var owner = ((HistoryService)Global.HistoryService).GetFileOwner(filename);
                if (owner != (int)Session["UserId"])
                {
                    Response.StatusCode = 403;
                    Response.End();
                    return;
                }
            }

            var fullPath = Path.Combine(Global.FilesPath, filename);
            if (!File.Exists(fullPath))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
            Response.TransmitFile(fullPath);
            Response.End();
        }
    }
}
