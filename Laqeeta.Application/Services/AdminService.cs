using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;

namespace Laqeeta.Application.Services;

public sealed class AdminService(IAdminRepository repository) : IAdminService
{
    public Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
        => repository.GetDashboardAsync(ct);

    public Task<IReadOnlyList<AdminEmployeeDto>> GetEmployeesAsync(
        string? search, CancellationToken ct = default)
        => repository.GetEmployeesAsync(search, ct);

    public async Task<ToggleEmployeeResult> ToggleEmployeeActiveAsync(
        long employeeId, long currentAdminId, CancellationToken ct = default)
    {
        if (employeeId == currentAdminId)
            return ToggleEmployeeResult.CannotChangeOwnAccount;

        var employee = await repository.GetEmployeeForUpdateAsync(employeeId, ct);
        if (employee is null)
            return ToggleEmployeeResult.NotFound;

        employee.IsActive = !employee.IsActive;
        await repository.SaveChangesAsync(ct);

        return ToggleEmployeeResult.Updated;
    }
}
