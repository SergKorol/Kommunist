using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kommunist.Core.Helpers;

public class EmailTokenGenerator
{
// Exactly 32 bytes = 256-bit key
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
    
    // Exactly 16 bytes = 128-bit IV
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("abcdefghijklmnop");

    public static string EncryptForBlobName(string email)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

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