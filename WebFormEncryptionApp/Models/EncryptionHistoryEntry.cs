namespace WebFormEncryptionApp.Models
{
    public class EncryptionHistoryEntry
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string MachineName { get; set; }
        public string DateTime { get; set; }
        public string SourcePath { get; set; }
        public string OutputPath { get; set; }
        public string Action { get; set; }
    }
}
