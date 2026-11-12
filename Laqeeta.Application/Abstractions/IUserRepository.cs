using Laqeeta.Core.DTOs;
using Laqeeta.Core.Models;


namespace Laqeeta.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default);
        Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<UserDto> GetProfileAsync(long userId, CancellationToken ct = default);
        Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken ct = default);
    }
}
