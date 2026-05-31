namespace WebFormEncryptionApp.Services
{
    public interface IEncryptionService
    {
        void EncryptFile(string inputFile, string outputFile, string password);
    }
}
