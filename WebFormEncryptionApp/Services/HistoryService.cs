using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly string _databasePath;

        public HistoryService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                var sql = @"CREATE TABLE IF NOT EXISTS EncryptionHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Filename TEXT NOT NULL,
                    MachineName TEXT NOT NULL,
                    DateTime TEXT NOT NULL,
                    SourcePath TEXT NOT NULL,
                    OutputPath TEXT NOT NULL,
                    Action TEXT NOT NULL,
                    UserId INTEGER NOT NULL DEFAULT 0)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Migration: add UserId column if missing
                using (var cmd = new SQLiteCommand("PRAGMA table_info(EncryptionHistory)", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    bool hasUserId = false;
                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == "UserId") hasUserId = true;
                    }
                    if (!hasUserId)
                    {
                        using (var alter = new SQLiteCommand("ALTER TABLE EncryptionHistory ADD COLUMN UserId INTEGER NOT NULL DEFAULT 0", conn))
                        {
                            alter.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public List<EncryptionHistoryEntry> LoadHistory(int userId)
        {
            var history = new List<EncryptionHistoryEntry>();
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM EncryptionHistory WHERE UserId = @uid ORDER BY DateTime DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add(ReadEntry(reader));
                        }
                    }
                }
            }
            return history;
        }

        public List<EncryptionHistoryEntry> LoadAllHistory()
        {
            var history = new List<EncryptionHistoryEntry>();
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM EncryptionHistory ORDER BY DateTime DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(ReadEntry(reader));
                    }
                }
            }
            return history;
        }

        public void SaveToHistory(string filename, string sourcePath, string outputPath, string action, int userId)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                var sql = @"INSERT INTO EncryptionHistory (Filename, MachineName, DateTime, SourcePath, OutputPath, Action, UserId)
                           VALUES (@Filename, @MachineName, @DateTime, @SourcePath, @OutputPath, @Action, @UserId)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Filename", filename);
                    cmd.Parameters.AddWithValue("@MachineName", Environment.MachineName);
                    cmd.Parameters.AddWithValue("@DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@SourcePath", sourcePath);
                    cmd.Parameters.AddWithValue("@OutputPath", outputPath);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ClearHistory(int userId)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM EncryptionHistory WHERE UserId = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ClearAllHistory()
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM EncryptionHistory", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetFilesByUser(int userId)
        {
            var files = new List<string>();
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT SourcePath, OutputPath FROM EncryptionHistory WHERE UserId = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            files.Add(reader["SourcePath"].ToString());
                            files.Add(reader["OutputPath"].ToString());
                        }
                    }
                }
            }
            return files;
        }

        public int GetFileOwner(string filename)
        {
            using (var conn = new SQLiteConnection("Data Source=" + _databasePath))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT UserId FROM EncryptionHistory WHERE SourcePath = @f OR OutputPath = @f LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("@f", filename);
                    var result = cmd.ExecuteScalar();
                    return result != null ? (int)(long)result : -1;
                }
            }
        }

        private static EncryptionHistoryEntry ReadEntry(SQLiteDataReader reader)
        {
            return new EncryptionHistoryEntry
            {
                Id = (int)(long)reader["Id"],
                Filename = reader["Filename"].ToString(),
                MachineName = reader["MachineName"].ToString(),
                DateTime = reader["DateTime"].ToString(),
                SourcePath = reader["SourcePath"].ToString(),
                OutputPath = reader["OutputPath"].ToString(),
                Action = reader["Action"].ToString()
            };
        }
    }
}
