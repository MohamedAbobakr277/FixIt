using System.Security.Cryptography;
using System.Text;

namespace FixIt.Common.Helpers;

public static class EncryptionHelper
{
    private static byte[]? _key;
    private static byte[]? _iv;

    public static void Initialize(string key, string iv)
    {
        _key = Encoding.UTF8.GetBytes(key);
        _iv = Encoding.UTF8.GetBytes(iv);
    }

    public static string Encrypt(string plainText)
    {
        if (_key == null || _iv == null)
            throw new InvalidOperationException("EncryptionHelper not initialized. Call Initialize() first.");

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
        {
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        if (_key == null || _iv == null)
            throw new InvalidOperationException("EncryptionHelper not initialized. Call Initialize() first.");

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream ms = new(Convert.FromBase64String(cipherText));
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);

        return sr.ReadToEnd();
    }
}
