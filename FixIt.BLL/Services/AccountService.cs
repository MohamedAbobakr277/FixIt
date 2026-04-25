using FixIt.Common.DTOs;
using FixIt.BLL.Interfaces;
using FixIt.Common.Constants;
using FixIt.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace FixIt.BLL.Services;

/// <summary>
/// Account service implementation handling authentication and registration.
/// </summary>
public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>?> RegisterAsync(RegisterDto dto)
    {
        var citizen = new Citizen
        {
            FullName    = dto.FullName.Trim(),
            UserName    = dto.Email.Trim().ToLower(),
            Email       = dto.Email.Trim().ToLower(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Address     = dto.Address?.Trim(),
            CreatedAt   = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(citizen, dto.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(citizen, AppConstants.CitizenRole);
            return null; // null = success
        }

        return result.Errors.Select(e => e.Description);
    }

    /// <inheritdoc/>
    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            userName: dto.Email.Trim().ToLower(),
            password: dto.Password,
            isPersistent: dto.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)  return null; // null = success
        if (result.IsLockedOut) return "Your account has been locked. Please try again later.";
        return "Invalid email or password. Please try again.";
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
