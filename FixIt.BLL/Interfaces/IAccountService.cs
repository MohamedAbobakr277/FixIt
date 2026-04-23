using FixIt.BLL.DTOs;

namespace FixIt.BLL.Interfaces;

public interface IAccountService
{
    /// <summary>Returns null on success, or a list of error messages on failure.</summary>
    Task<IEnumerable<string>?> RegisterAsync(RegisterDto dto);

    /// <summary>Returns null on success, or an error message on failure.</summary>
    Task<string?> LoginAsync(LoginDto dto);

    Task LogoutAsync();
}
