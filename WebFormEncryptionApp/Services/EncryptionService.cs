using System.IO;
using System.Security.Cryptography;

namespace WebFormEncryptionApp.Services
{
    public class EncryptionService : IEncryptionService
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        public void EncryptFile(string inputFile, string outputFile, string password)
        {
            var salt = GenerateSalt();
            var iv = GenerateIv();

            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var key = DeriveKey(password, salt);
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor())
                using (var fs = new FileStream(outputFile, FileMode.Create))
                using (var cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                {
                    fs.Write(salt, 0, salt.Length);
                    fs.Write(iv, 0, iv.Length);

                    using (var reader = new FileStream(inputFile, FileMode.Open))
                    {
                        reader.CopyTo(cs);
                    }
                }
            }
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] GenerateIv()
        {
            var iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        private byte[] DeriveKey(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(KeySize / 8);
            }
        }
    }
}
