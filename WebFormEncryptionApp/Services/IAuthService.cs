using System.Collections.Generic;
using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public interface IAuthService
    {
        void InitializeDatabase();
        void SeedAdmin(string username, string password);
        User ValidateLogin(string username, string password);
        User GetUserById(int id);
        List<User> GetAllUsers();
        void CreateUser(string username, string password, bool isAdmin);
        void DeleteUser(int userId);
    }
}
