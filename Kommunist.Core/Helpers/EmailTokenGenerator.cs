using System.Security.Cryptography;

namespace Kommunist.Core.Helpers;

public static class EmailTokenGenerator
{
    private static readonly byte[] Key = "12345678901234567890123456789012"u8.ToArray();
    
    private static readonly byte[] Iv = "abcdefghijklmnop"u8.ToArray();

    public static string EncryptForBlobName(string email)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(email);
        }

        var encrypted = ms.ToArray();
        var base64 = Convert.ToBase64String(encrypted);

        // Make base64 URL-safe (blob-safe)
        var safeToken = base64
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .ToLowerInvariant();

        return safeToken;
    }
}