namespace FixIt.BLL.Interfaces;

public interface ITwoFactorService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey);
    string GenerateQrCodeImageBase64(string qrCodeUri);
    bool VerifyCode(string secretKey, string code);
    IEnumerable<string> GenerateRecoveryCodes(int count = 10);
}
