using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public interface IValidationService
    {
        ValidationResult ValidateInputs(string sourceFile, string password, string outputFile);
    }
}
