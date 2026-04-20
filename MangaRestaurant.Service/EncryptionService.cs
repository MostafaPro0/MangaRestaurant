using System.Security.Cryptography;
using System.Text;
using System.IO;
using MangaRestaurant.Core.Service;
using Microsoft.Extensions.Configuration;

namespace MangaRestaurant.Service
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration)
        {
            var sharedKey = "MoMangaMoatasem";
            var keyBytes = Encoding.UTF8.GetBytes(sharedKey);
            
            // AES-128 requires exactly 16 bytes (128 bits)
            _key = new byte[16];
            Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, 16));
            
            _iv = new byte[16];
            Array.Copy(keyBytes, _iv, Math.Min(keyBytes.Length, 16));
        }

        public string GetPublicKey()
        {
            // Not needed for AES but keeping the interface method
            return "MoMangaMoatasem";
        }

        public string Decrypt(string encryptedText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using var decryptor = aes.CreateDecryptor(_key, _iv);
                var cipherTextBytes = Convert.FromBase64String(encryptedText);

                using var ms = new MemoryStream(cipherTextBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new Exception("AES Decryption failed. " + ex.Message);
            }
        }
    }
}
