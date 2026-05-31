namespace WebFormEncryptionApp.Services
{
    public interface IDecryptionService
    {
        void DecryptFile(string inputFile, string outputFile, string password);
    }
}
