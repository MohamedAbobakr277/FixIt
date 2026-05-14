using FixIt.Common.DTOs;
using FixIt.BLL.DTOs;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace FixIt.BLL.Interfaces;

public interface IAccountService
{
    /// <summary>Returns null on success, or a list of error messages on failure.</summary>
    Task<(IEnumerable<string>? Errors, string? UserId, string? Token)> RegisterAsync(RegisterDto dto);

    /// <summary>Returns null on success, or an error message on failure.</summary>
    Task<string?> LoginAsync(FixIt.Common.DTOs.LoginDto dto);

    Task<ApplicationUser?> GetUserByEmailAsync(string email);

    Task<bool> ConfirmEmailAsync(string userId, string token);

    Task LogoutAsync();
    /// <summary>
    /// Initiates the forgot‑password flow. Returns (success, token, userId). If the email is not found or not confirmed, returns (true, null, null) to avoid enumeration.
    /// </summary>
    Task<(bool success, string? token, string? userId)> ForgotPasswordAsync(string email);

    /// <summary>
    /// Resets the password using the supplied token.
    /// </summary>
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);


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
    Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<IEnumerable<LoginActivityDto>> GetLoginActivityAsync(string userId);
}
