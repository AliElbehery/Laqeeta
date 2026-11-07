using Laqeeta.Core.DTOs;

namespace Laqeeta.Core.Interfaces;

public interface IAuthService
{
    /// <summary>Public self-registration. Always creates a passenger (role = User).</summary>
    Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);

    Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<UserDto> GetProfileAsync(long userId, CancellationToken ct = default);
    Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken ct = default);

    /// <summary>Employees are never self-registered; an existing employee creates them.</summary>
    Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default);
}
