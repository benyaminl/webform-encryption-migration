using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using WebFormEncryptionCore.Models;

namespace WebFormEncryptionCore.Services;

public class AuthService : IAuthService
{
    private readonly string _connectionString;

    public AuthService(IConfiguration config)
    {
        var dbPath = Path.GetFullPath(config["DatabasePath"]!);
        _connectionString = $"Data Source={dbPath}";
    }

    public List<User> GetAllUsers()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, IsAdmin FROM Users ORDER BY Username";
        using var reader = cmd.ExecuteReader();
        var users = new List<User>();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                IsAdmin = reader.GetInt32(2) == 1
            });
        }
        return users;
    }

    public void CreateUser(string username, string password, bool isAdmin)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Salt, IsAdmin, CreatedAt) VALUES (@u, @h, @s, @a, @c)";
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@h", hash);
        cmd.Parameters.AddWithValue("@s", salt);
        cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
        cmd.Parameters.AddWithValue("@c", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public void UpdateUser(int userId, bool isAdmin, string? newPassword)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        if (!string.IsNullOrEmpty(newPassword))
        {
            var salt = GenerateSalt();
            var hash = HashPassword(newPassword, salt);
            cmd.CommandText = "UPDATE Users SET IsAdmin = @a, PasswordHash = @h, Salt = @s WHERE Id = @id";
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
        }
        else
        {
            cmd.CommandText = "UPDATE Users SET IsAdmin = @a WHERE Id = @id";
        }
        cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteUser(int userId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    private static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100000, HashAlgorithmName.SHA1, 32);
        return Convert.ToBase64String(hash);
    }
}
