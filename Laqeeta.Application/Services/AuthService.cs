using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;


namespace Laqeeta.Infrastructure.Services;

public class AuthService(IUserRepository _repository) : IAuthService
{

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        return await _repository.RegisterAsync(dto, ct);
    }

    public async Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default)
    {
        return await _repository.CreateEmployeeAsync(dto, ct);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
       return await _repository.LoginAsync(dto, ct);
    }

    public async Task<UserDto> GetProfileAsync(long userId, CancellationToken ct = default)
    {
        return await _repository.GetProfileAsync(userId, ct);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        await _repository.ChangePasswordAsync(userId, dto, ct);
    }

}