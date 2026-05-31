using WebFormEncryptionApp.Models;

namespace WebFormEncryptionApp.Services
{
    public class ValidationService : IValidationService
    {
        public ValidationResult ValidateInputs(string sourceFile, string password, string outputFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
                return new ValidationResult(false, "Please select a source file.");
            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult(false, "Please enter a password.");
            if (string.IsNullOrWhiteSpace(outputFile))
                return new ValidationResult(false, "Please specify an output filename.");
            return new ValidationResult(true, null);
        }
    }
}
