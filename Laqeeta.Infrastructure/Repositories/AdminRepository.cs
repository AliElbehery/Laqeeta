using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public sealed class AdminRepository(AppDbContext db) : IAdminRepository
{
    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
        => new()
        {
            TotalEmployees = await db.Users.CountAsync(x => x.Role == UserRole.Employee, ct),
            ActiveEmployees = await db.Users.CountAsync(x => x.Role == UserRole.Employee && x.IsActive, ct),
            TotalPassengers = await db.Users.CountAsync(x => x.Role == UserRole.User, ct),
            TotalStations = await db.Stations.CountAsync(ct),
            TotalLostReports = await db.LostReports.CountAsync(ct),
            OpenLostReports = await db.LostReports.CountAsync(x => x.Status == LostReportStatus.Open, ct),
            TotalFoundItems = await db.FoundItems.CountAsync(ct),
            ItemsInStorage = await db.FoundItems.CountAsync(x => x.ItemStatus == FoundItemStatus.InStorage, ct),
            PendingClaims = await db.OwnershipClaims.CountAsync(x => x.ClaimStatus == ClaimStatus.Pending, ct),
            SuggestedMatches = await db.PossibleMatches.CountAsync(x => x.MatchStatus == MatchStatus.Suggested, ct)
        };

    public async Task<IReadOnlyList<AdminEmployeeDto>> GetEmployeesAsync(
        string? search, CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking()
            .Where(x => x.Role == UserRole.Employee);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FullName.Contains(term)
                || x.Username.Contains(term) || x.Email.Contains(term)
                || (x.Station != null && x.Station.StationName.Contains(term)));
        }

        return await query
            .OrderBy(x => x.FullName)
            .Select(x => new AdminEmployeeDto(
                x.UserId,
                x.FullName,
                x.Username,
                x.Email,
                x.Phone,
                x.Station == null ? null : x.Station.StationName,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(ct);
    }

    public Task<User?> GetEmployeeForUpdateAsync(long employeeId, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(
            x => x.UserId == employeeId && x.Role == UserRole.Employee, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
