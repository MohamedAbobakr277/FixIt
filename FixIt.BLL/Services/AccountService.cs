using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using FixIt.BLL.DTOs;
using FixIt.Common.Helpers;

namespace FixIt.BLL.Services;

/// <summary>
/// Account service implementation handling authentication and registration.
/// </summary>
public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITwoFactorService _twoFactorService;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFactorService = twoFactorService;
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<string>? Errors, string? UserId, string? Token)> RegisterAsync(RegisterDto dto)
    {
        var citizen = new Citizen
        {
            FullName = dto.FullName.Trim(),
            UserName = dto.Email.Trim().ToLower(),
            Email = dto.Email.Trim().ToLower(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Address = dto.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(citizen, dto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(citizen, AppConstants.CitizenRole);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(citizen);
            return (null, citizen.Id, token); // null = success
        }

        return (result.Errors.Select(e => e.Description), null, null);
    }

    /// <inheritdoc/>
    public async Task<string?> LoginAsync(FixIt.Common.DTOs.LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim().ToLower());
        if (user == null) return "Invalid email or password.";

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return "Please check your email and confirm your account before signing in.";
        }

        if (user.IsTwoFactorEnabled)
        {
            // Just check password first, don't sign in yet if 2FA is required
            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);
                return "Invalid email or password.";
            }

            if (await _userManager.IsLockedOutAsync(user))
                return "Your account has been locked. Please try again later.";

            return "REQUIRES_2FA";
        }

        var result = await _signInManager.PasswordSignInAsync(
            userName: dto.Email.Trim().ToLower(),
            password: dto.Password,
            isPersistent: dto.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded) return null; // null = success
        if (result.IsLockedOut) return "Your account has been locked. Please try again later.";
        
        return "Invalid email or password. Please try again.";
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<TwoFactorSetupDto> GenerateTwoFactorSetupAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        string secretKey;

        // Only generate a new secret if one doesn't already exist
        // This prevents the QR code from changing on every page refresh
        if (string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            secretKey = _twoFactorService.GenerateSecretKey();
            user.TwoFactorSecret = EncryptionHelper.Encrypt(secretKey);
            await _userManager.UpdateAsync(user);
        }
        else
        {
            secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
        }

        var qrUri = _twoFactorService.GenerateQrCodeUri(user.Email!, secretKey);
        var qrBase64 = _twoFactorService.GenerateQrCodeImageBase64(qrUri);

        return new TwoFactorSetupDto
        {
            QrCodeImageUrl = qrBase64,
            ManualEntryKey = secretKey
        };
    }

    public async Task<bool> EnableTwoFactorAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        var secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
        if (_twoFactorService.VerifyCode(secretKey, code.Trim()))
        {
            user.IsTwoFactorEnabled = true;
            
            // Generate recovery codes if not already present
            if (string.IsNullOrEmpty(user.RecoveryCodes))
            {
                var codes = _twoFactorService.GenerateRecoveryCodes();
                user.RecoveryCodes = string.Join(";", codes);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // 1. Rotate security stamp → invalidates all OTHER sessions (other devices)
                await _userManager.UpdateSecurityStampAsync(user);
                // 2. Refresh THIS session's cookie with the new stamp + fresh claims
                //    so the current user stays logged in with updated data
                await _signInManager.RefreshSignInAsync(user);
            }
            return result.Succeeded;
        }

        return false;
    }

    public async Task<bool> DisableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.IsTwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        user.RecoveryCodes = null;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // 1. Rotate security stamp → invalidates all OTHER sessions (other devices)
            await _userManager.UpdateSecurityStampAsync(user);
            // 2. Refresh THIS session's cookie with the new stamp + fresh claims
            await _signInManager.RefreshSignInAsync(user);
        }
        return result.Succeeded;
    }

    public async Task<bool> VerifyTwoFactorTokenAsync(string userId, string code, bool rememberMe = false)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return await VerifyUserTwoFactorTokenAsync(user, code, rememberMe);
    }

    public async Task<bool> VerifyTwoFactorTokenByEmailAsync(string email, string code, bool rememberMe = false)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return await VerifyUserTwoFactorTokenAsync(user, code, rememberMe);
    }

    private async Task<bool> VerifyUserTwoFactorTokenAsync(ApplicationUser? user, string code, bool rememberMe = false)
    {
        if (user == null || !user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret)) return false;

        var secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
        var isValid = _twoFactorService.VerifyCode(secretKey, code.Trim());
        
        if (isValid)
        {
            await _signInManager.SignInAsync(user, isPersistent: rememberMe);
        }
        else
        {
            await _userManager.AccessFailedAsync(user);
        }

        return isValid;
    }

    public async Task<IEnumerable<string>> GenerateRecoveryCodesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();

        var codes = _twoFactorService.GenerateRecoveryCodes();
        user.RecoveryCodes = string.Join(";", codes);
        await _userManager.UpdateAsync(user);

        return codes;
    }

    public async Task<IEnumerable<string>?> GetRecoveryCodesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RecoveryCodes)) return null;

        return user.RecoveryCodes.Split(';').ToList();
    }

    public async Task<bool> RedeemRecoveryCodeAsync(string userId, string code, bool rememberMe = false)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.RecoveryCodes)) return false;

        var codes = user.RecoveryCodes.Split(';').ToList();
        if (codes.Contains(code.ToUpper()))
        {
            codes.Remove(code.ToUpper());
            user.RecoveryCodes = string.Join(";", codes);
            await _userManager.UpdateAsync(user);
            
            await _signInManager.SignInAsync(user, isPersistent: rememberMe);
            return true;
        }

        return false;
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.IsTwoFactorEnabled ?? false;
    }
}
