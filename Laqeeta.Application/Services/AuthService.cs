using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;

namespace Laqeeta.Application.Services;

public class AuthService(IUserRepository repository) : IAuthService
{
    public Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
        => repository.RegisterAsync(dto, ct);

    public Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default)
        => repository.CreateEmployeeAsync(dto, ct);

    public Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
        => repository.LoginAsync(dto, ct);

    public Task<UserDto> GetProfileAsync(long userId, CancellationToken ct = default)
        => repository.GetProfileAsync(userId, ct);

    public Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken ct = default)
        => repository.ChangePasswordAsync(userId, dto, ct);
}
