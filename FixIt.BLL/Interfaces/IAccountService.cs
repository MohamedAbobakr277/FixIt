using FixIt.Common.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IAccountService
{
    /// <summary>Returns null on success, or a list of error messages on failure.</summary>
    Task<(IEnumerable<string>? Errors, string? UserId, string? Token)> RegisterAsync(RegisterDto dto);

    /// <summary>Returns null on success, or an error message on failure.</summary>
    Task<string?> LoginAsync(LoginDto dto);

    Task<bool> ConfirmEmailAsync(string userId, string token);

    Task LogoutAsync();
}
