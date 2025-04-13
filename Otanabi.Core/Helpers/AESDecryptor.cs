using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace Otanabi.Core.Helpers;

public class AESDecryptor
{
    public static string DecryptLink(string encryptedLinkBase64, string secretKey, bool isUtf8 = false)
    {
        try
        {
            if (isUtf8)
            {
                return DecryptLinkUtf8(encryptedLinkBase64, secretKey);
            }

            var encryptedData = Convert.FromBase64String(encryptedLinkBase64);

            var iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);

            var cipherText = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, cipherText, 0, cipherText.Length);

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al descifrar: " + ex.Message);
            return null;
        }
    }

    private static string DecryptLinkUtf8(string encryptedBase64, string secretKey)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var encryptedBytes = Convert.FromBase64String(encryptedBase64);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al descifrar: " + ex.Message);
            return null;
        }
    }
}
