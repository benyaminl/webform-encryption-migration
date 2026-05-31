using System;
using System.Configuration;
using System.IO;
using System.Web;
using WebFormEncryptionApp.Services;

namespace WebFormEncryptionApp
{
    public class Global : HttpApplication
    {
        public static IHistoryService HistoryService { get; private set; }
        public static IEncryptionService EncryptionService { get; private set; }
        public static IDecryptionService DecryptionService { get; private set; }
        public static IValidationService ValidationService { get; private set; }
        public static IAuthService AuthService { get; private set; }
        public static string FilesPath { get; private set; }

        void Application_Start(object sender, EventArgs e)
        {
            var dbPath = HttpContext.Current.Server.MapPath(
                ConfigurationManager.AppSettings["DatabasePath"]);
            FilesPath = HttpContext.Current.Server.MapPath(
                ConfigurationManager.AppSettings["FilesPath"]);
            Directory.CreateDirectory(FilesPath);

            HistoryService = new HistoryService(dbPath);
            HistoryService.InitializeDatabase();

            AuthService = new AuthService(dbPath);
            AuthService.InitializeDatabase();
            AuthService.SeedAdmin("admin", "admin123");

            EncryptionService = new EncryptionService();
            DecryptionService = new DecryptionService();
            ValidationService = new ValidationService();
        }
    }
}
