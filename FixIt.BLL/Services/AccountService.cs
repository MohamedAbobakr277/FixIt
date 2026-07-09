using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using FixIt.BLL.DTOs;
using FixIt.Common.Helpers;
using FixIt.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FixIt.BLL.Services;

/// <summary>
/// Account service implementation handling authentication and registration.
/// </summary>
public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITwoFactorService twoFactorService,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _twoFactorService = twoFactorService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<string>? Errors, string? UserId, string? Token)> RegisterAsync(RegisterDto dto)
    {
        ApplicationUser user;
        string assignedRole;

        if (dto.Role == AppConstants.AdminRole)
        {
            user = new Admin
            {
                FullName = dto.FullName.Trim(),
                UserName = dto.Email.Trim().ToLower(),
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            assignedRole = AppConstants.AdminRole;
        }
        else
        {
            user = new Citizen
            {
                FullName = dto.FullName.Trim(),
                UserName = dto.Email.Trim().ToLower(),
                Email = dto.Email.Trim().ToLower(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Address = dto.Address?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            assignedRole = AppConstants.CitizenRole;
        }

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, assignedRole);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return (null, user.Id, token); // null = success
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

        if (result.Succeeded)
        {
            await RecordLoginAsync(user.Id);
            return null; // null = success
        }
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
            try
            {
                secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // The encryption key must have changed or the secret is invalid. Reset it.
                secretKey = _twoFactorService.GenerateSecretKey();
                user.TwoFactorSecret = EncryptionHelper.Encrypt(secretKey);
                await _userManager.UpdateAsync(user);
            }
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

        string secretKey;
        try
        {
            secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return false;
        }

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

        string secretKey;
        try
        {
            secretKey = EncryptionHelper.Decrypt(user.TwoFactorSecret);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return false;
        }

        var isValid = _twoFactorService.VerifyCode(secretKey, code.Trim());
        
        if (isValid)
        {
            await _signInManager.SignInAsync(user, isPersistent: rememberMe);
            await RecordLoginAsync(user.Id);
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
            await RecordLoginAsync(user.Id);
            return true;
        }

        return false;
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.IsTwoFactorEnabled ?? false;
    }

    // Added ForgotPasswordAsync implementation
    public async Task<(bool success, string? token, string? userId)> ForgotPasswordAsync(string email)
    {
        // Use the email provided directly (Identity handles normalization)
        var user = await _userManager.FindByEmailAsync(email.Trim());
        // If user not found or email not confirmed, silently succeed without token to avoid enumeration
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return (true, null, null);
        }
        // Generate reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return (true, token, user.Id);
    }

    // Added ResetPasswordAsync implementation
    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (result.Succeeded)
        {
            // Automatically confirm email if password reset was successful via email link
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            return true;
        }
        return false;
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
        }
        return result;
    }

    public async Task<IEnumerable<LoginActivityDto>> GetLoginActivityAsync(string userId)
    {
        var logs = await _unitOfWork.LoginHistories
            .GetAll()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(10)
            .ToListAsync();

        return logs.Select(l => new LoginActivityDto
        {
            Device = l.Device,
            Location = l.Location,
            IpAddress = l.IpAddress,
            Timestamp = l.Timestamp,
            IsCurrentSession = (DateTime.UtcNow - l.Timestamp).TotalMinutes < 60 // Simplification for demo
        });
    }

    private async Task RecordLoginAsync(string userId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Basic UserAgent parsing for demo
        string device = "Unknown Device";
        if (userAgent.Contains("Windows")) device = "Chrome on Windows";
        else if (userAgent.Contains("iPhone")) device = "iPhone";
        else if (userAgent.Contains("Android")) device = "Android Device";
        else if (userAgent.Contains("Macintosh")) device = "MacBook / iMac";

        var history = new LoginHistory
        {
            UserId = userId,
            Device = device,
            IpAddress = ipAddress,
            Location = "Cairo, Egypt", // In real app, use IP-to-Location service
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.LoginHistories.AddAsync(history);
        await _unitOfWork.CompleteAsync();
    }
}
