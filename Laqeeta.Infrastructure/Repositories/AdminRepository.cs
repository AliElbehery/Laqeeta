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
    {
        // Aggregate each table in one query instead of issuing a separate COUNT for
        // every card on the dashboard. This reduces database round-trips from 10 to 6.
        // Grouping by the persisted enum/status columns also lets the existing status
        // indexes on lost_reports and found_items support their dashboard counts.
        var userCounts = await db.Users
            .AsNoTracking()
            .GroupBy(x => new { x.Role, x.IsActive })
            .Select(g => new
            {
                g.Key.Role,
                g.Key.IsActive,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var lostReportCounts = await db.LostReports
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var foundItemCounts = await db.FoundItems
            .AsNoTracking()
            .GroupBy(x => x.ItemStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var claimCounts = await db.OwnershipClaims
            .AsNoTracking()
            .GroupBy(x => x.ClaimStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var matchCounts = await db.PossibleMatches
            .AsNoTracking()
            .GroupBy(x => x.MatchStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var totalStations = await db.Stations.CountAsync(ct);

        return new AdminDashboardDto
        {
            TotalEmployees = userCounts
                .Where(x => x.Role == UserRole.Employee)
                .Sum(x => x.Count),
            ActiveEmployees = userCounts
                .Where(x => x.Role == UserRole.Employee && x.IsActive)
                .Sum(x => x.Count),
            TotalPassengers = userCounts
                .Where(x => x.Role == UserRole.User)
                .Sum(x => x.Count),
            TotalStations = totalStations,
            TotalLostReports = lostReportCounts.Sum(x => x.Count),
            OpenLostReports = lostReportCounts
                .Where(x => x.Status == LostReportStatus.Open)
                .Sum(x => x.Count),
            TotalFoundItems = foundItemCounts.Sum(x => x.Count),
            ItemsInStorage = foundItemCounts
                .Where(x => x.Status == FoundItemStatus.InStorage)
                .Sum(x => x.Count),
            PendingClaims = claimCounts
                .Where(x => x.Status == ClaimStatus.Pending)
                .Sum(x => x.Count),
            SuggestedMatches = matchCounts
                .Where(x => x.Status == MatchStatus.Suggested)
                .Sum(x => x.Count)
        };
    }

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
