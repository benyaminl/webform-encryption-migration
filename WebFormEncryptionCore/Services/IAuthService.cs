using WebFormEncryptionCore.Models;

namespace WebFormEncryptionCore.Services;

public interface IAuthService
{
    List<User> GetAllUsers();
    void CreateUser(string username, string password, bool isAdmin);
    void UpdateUser(int userId, bool isAdmin, string? newPassword);
    void DeleteUser(int userId);
}
