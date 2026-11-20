using Laqeeta.Core.DTOs;

namespace Laqeeta.Core.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminEmployeeDto>> GetEmployeesAsync(string? search, CancellationToken ct = default);
    Task<ToggleEmployeeResult> ToggleEmployeeActiveAsync(long employeeId, long currentAdminId,
        CancellationToken ct = default);
}
