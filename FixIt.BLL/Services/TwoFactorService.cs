using FixIt.BLL.Interfaces;
using OtpNet;
using QRCoder;
using System.Security.Cryptography;

namespace FixIt.BLL.Services;

public class TwoFactorService : ITwoFactorService
{
    private const string Issuer = "FixItApp";

    public string GenerateSecretKey()
    {
        byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKey);
    }

    public string GenerateQrCodeUri(string email, string secretKey)
    {
        return $"otpauth://totp/{Issuer}:{email}?secret={secretKey}&issuer={Issuer}&digits=6";
    }

    public string GenerateQrCodeImageBase64(string qrCodeUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeImage = qrCode.GetGraphic(20);
        return $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
    }

    public bool VerifyCode(string secretKey, string code)
    {
        try
        {
            byte[] secretKeyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretKeyBytes);
            return totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<string> GenerateRecoveryCodes(int count = 10)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            codes.Add(Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper());
        }
        return codes;
    }
}
