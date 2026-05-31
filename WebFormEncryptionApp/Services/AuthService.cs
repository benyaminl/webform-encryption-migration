using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _databasePath;

        public AuthService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                var sql = @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL,
                    IsAdmin INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SeedAdmin(string username, string password)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE Username = @u", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    if ((long)cmd.ExecuteScalar() > 0) return;
                }

                var salt = GenerateSalt();
                var hash = HashPassword(password, salt);
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Users (Username, PasswordHash, Salt, IsAdmin, CreatedAt) VALUES (@u, @h, @s, 1, @c)", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@h", hash);
                    cmd.Parameters.AddWithValue("@s", salt);
                    cmd.Parameters.AddWithValue("@c", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public User ValidateLogin(string username, string password)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Users WHERE Username = @u", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        var storedHash = reader["PasswordHash"].ToString();
                        var salt = reader["Salt"].ToString();
                        if (HashPassword(password, salt) != storedHash) return null;
                        return new User
                        {
                            Id = (int)(long)reader["Id"],
                            Username = reader["Username"].ToString(),
                            PasswordHash = storedHash,
                            Salt = salt,
                            IsAdmin = (long)reader["IsAdmin"] == 1
                        };
                    }
                }
            }
        }

        public User GetUserById(int id)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Users WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return new User
                        {
                            Id = (int)(long)reader["Id"],
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Salt = reader["Salt"].ToString(),
                            IsAdmin = (long)reader["IsAdmin"] == 1
                        };
                    }
                }
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT Id, Username, IsAdmin FROM Users ORDER BY Username", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = (int)(long)reader["Id"],
                            Username = reader["Username"].ToString(),
                            IsAdmin = (long)reader["IsAdmin"] == 1
                        });
                    }
                }
            }
            return users;
        }

        public void CreateUser(string username, string password, bool isAdmin)
        {
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Users (Username, PasswordHash, Salt, IsAdmin, CreatedAt) VALUES (@u, @h, @s, @a, @c)", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@h", hash);
                    cmd.Parameters.AddWithValue("@s", salt);
                    cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
                    cmd.Parameters.AddWithValue("@c", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUser(int userId, bool isAdmin, string newPassword)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                if (!string.IsNullOrEmpty(newPassword))
                {
                    var salt = GenerateSalt();
                    var hash = HashPassword(newPassword, salt);
                    using (var cmd = new SQLiteCommand("UPDATE Users SET IsAdmin = @a, PasswordHash = @h, Salt = @s WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
                        cmd.Parameters.AddWithValue("@h", hash);
                        cmd.Parameters.AddWithValue("@s", salt);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmd = new SQLiteCommand("UPDATE Users SET IsAdmin = @a WHERE Id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@a", isAdmin ? 1 : 0);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteUser(int userId)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Users WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string GenerateSalt()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        private static string HashPassword(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000))
            {
                var hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
