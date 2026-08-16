using System.Security.Cryptography;
using System.Text;

namespace LogMate.Common.Crypto;

public static class AesTokenCipher
{
    private static readonly string Key = Environment.GetEnvironmentVariable("AES_KEY")!;

    public static (string Version, string DecryptedText) Decrypt(string payload)
    {
        var parts = payload.Split(':');
        if (parts.Length != 3)
            throw new ArgumentException("Invalid payload format");

        string version = parts[0];
        string encryptedData = parts[1];
        string ivBase64 = parts[2];

        byte[] keyBytes = Encoding.UTF8.GetBytes(Key);
        byte[] ivBytes = Convert.FromBase64String(ivBase64);
        byte[] cipherBytes = Convert.FromBase64String(encryptedData);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = keyBytes;
        aesAlg.IV = ivBytes;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

        using ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return (version, Encoding.UTF8.GetString(decryptedBytes));
    }
}

public static class PepperHasher
{
    public static string HashWithPepper(string id, string pepper)
    {
        string combinedInput = id + pepper;
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedInput));
        return Convert.ToBase64String(hashBytes);
    }
}
