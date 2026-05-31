using System.IO;
using System.Security.Cryptography;

namespace WebFormEncryptionApp.Services
{
    public class DecryptionService : IDecryptionService
    {
        private const int KeySize = 256;

        public void DecryptFile(string inputFile, string outputFile, string password)
        {
            using (var fs = new FileStream(inputFile, FileMode.Open))
            {
                var salt = new byte[16];
                var iv = new byte[16];

                fs.Read(salt, 0, salt.Length);
                fs.Read(iv, 0, iv.Length);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    var key = DeriveKey(password, salt);
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                    using (var writer = new FileStream(outputFile, FileMode.Create))
                    {
                        cs.CopyTo(writer);
                    }
                }
            }
        }

        private byte[] DeriveKey(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(KeySize / 8);
            }
        }
    }
}
