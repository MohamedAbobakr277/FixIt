namespace FixIt.BLL.DTOs;

public class TwoFactorSetupDto
{
    public string? QrCodeImageUrl { get; set; }
    public string? ManualEntryKey { get; set; }
}

public class TwoFactorVerifyDto
{
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class TwoFactorRecoveryDto
{
    public IEnumerable<string> RecoveryCodes { get; set; } = new List<string>();
}
