using System.Collections.Generic;
using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public interface IHistoryService
    {
        void InitializeDatabase();
        List<EncryptionHistoryEntry> LoadHistory(int userId);
        List<EncryptionHistoryEntry> LoadAllHistory();
        void SaveToHistory(string filename, string sourcePath, string outputPath, string action, int userId);
        void ClearHistory(int userId);
        void ClearAllHistory();
    }
}
