using System;
using System.Web.UI;

namespace WebFormEncryptionApp
{
    public partial class UsersPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Strangler Fig: redirect to new .NET Core MVC route
            Response.Redirect("http://localhost:5000/Users");
        }
    }
}
