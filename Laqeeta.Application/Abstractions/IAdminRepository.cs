using Laqeeta.Core.DTOs;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

public interface IAdminRepository
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminEmployeeDto>> GetEmployeesAsync(string? search, CancellationToken ct = default);
    Task<User?> GetEmployeeForUpdateAsync(long employeeId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
