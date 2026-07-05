using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public partial class LogMateFunction
    {
        public enum UserMode
        {
            Service = 0,
            Guest = 1,
        }

        private readonly ILogger<LogMateFunction> _logger;
        private readonly string BaseLogm8Url = "https://logm8.com";
        private readonly string key = Environment.GetEnvironmentVariable("AES_KEY")!;
        private readonly string pepper = Environment.GetEnvironmentVariable("HASH_PEPPER")!;

        public LogMateFunction(ILogger<LogMateFunction> logger)
        {
            _logger = logger;
        }

        private string GenerateAccessToken() => Guid.NewGuid().ToString();

        public (string version, string decryptedText) DecryptString(string payload)
        {
            var parts = payload.Split(':');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid payload format");

            string version = parts[0];
            string encryptedData = parts[1];
            string ivBase64 = parts[2];

            byte[] keyBytes = Encoding.UTF8.GetBytes(this.key);
            byte[] ivBytes = Convert.FromBase64String(ivBase64);
            byte[] cipherBytes = Convert.FromBase64String(encryptedData);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(
                        cipherBytes,
                        0,
                        cipherBytes.Length
                    );
                    return (version, Encoding.UTF8.GetString(decryptedBytes));
                }
            }
        }

        private static string DecryptText(RSA rsa, string encryptedText, string privateKey)
        {
            rsa.FromXmlString(privateKey);
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private string EncryptText(RSA rsa, string id, string publicKey)
        {
            rsa.FromXmlString(publicKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(id);
            byte[] encryptedBytes = rsa.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string HashWithPepper(string id, string pepper)
        {
            string combinedInput = id + pepper;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static async Task<HttpResponseData> CreateResponseAsync(
            HttpRequestData req,
            HttpStatusCode status,
            string message
        )
        {
            var response = req.CreateResponse(status);
            await response.WriteStringAsync(message);
            return response;
        }
    }
}
