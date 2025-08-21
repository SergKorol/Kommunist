using System.Security.Cryptography;

namespace Kommunist.Core.Helpers;

public static class EmailTokenGenerator
{
    private static readonly byte[] Salt = "KommunistAppSalt"u8.ToArray();

    private const int KeySize = 32;
    private const int IvSize = 16;
    private const int Iterations = 10000;

    public static string EncryptForBlobName(string email, string password = null)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        password ??= "DefaultAppEncryptionKey2025";

        using var deriveBytes = new Rfc2898DeriveBytes(password, Salt, Iterations, HashAlgorithmName.SHA256);
        var key = deriveBytes.GetBytes(KeySize);
        var iv = deriveBytes.GetBytes(IvSize);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(email);
        }

        var encrypted = ms.ToArray();
        var base64 = Convert.ToBase64String(encrypted);

        var safeToken = base64
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .ToLowerInvariant();

        return safeToken;
    }
}