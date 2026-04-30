using FixIt.Common.DTOs;
using FixIt.BLL.DTOs;
using FixIt.DAL.Entities;

namespace FixIt.BLL.Interfaces;

public interface IAccountService
{
    /// <summary>Returns null on success, or a list of error messages on failure.</summary>
    Task<IEnumerable<string>?> RegisterAsync(FixIt.Common.DTOs.RegisterDto dto);

    /// <summary>Returns null on success, or an error message on failure.</summary>
    Task<string?> LoginAsync(FixIt.Common.DTOs.LoginDto dto);

    Task<ApplicationUser?> GetUserByEmailAsync(string email);

    Task LogoutAsync();

    // 2FA Methods
    Task<TwoFactorSetupDto> GenerateTwoFactorSetupAsync(string userId);
    Task<bool> EnableTwoFactorAsync(string userId, string code);
    Task<bool> DisableTwoFactorAsync(string userId);
    Task<bool> VerifyTwoFactorTokenAsync(string userId, string code, bool rememberMe = false);
    Task<bool> VerifyTwoFactorTokenByEmailAsync(string email, string code, bool rememberMe = false);
    Task<IEnumerable<string>> GenerateRecoveryCodesAsync(string userId);
    Task<IEnumerable<string>?> GetRecoveryCodesAsync(string userId);
    Task<bool> RedeemRecoveryCodeAsync(string userId, string code, bool rememberMe = false);
    Task<bool> IsTwoFactorEnabledAsync(string userId);
}
