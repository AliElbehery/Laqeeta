using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public class LostReportRepository(AppDbContext db) : ILostReportRepository
{
    public Task<User?> GetUserAsync(long userId, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(user => user.UserId == userId, ct);

    public Task<bool> MetroLineExistsAsync(long lineId, CancellationToken ct = default)
        => db.MetroLines.AnyAsync(line => line.LineId == lineId, ct);

    public Task AddAsync(LostReport report, CancellationToken ct = default)
        => db.LostReports.AddAsync(report, ct).AsTask();

    public async Task<IReadOnlyList<LostReport>> GetByUserAsync(
        long userId,
        CancellationToken ct = default)
        => await db.LostReports
            .AsNoTracking()
            .Where(report => report.UserId == userId)
            .Include(report => report.Line)
            .OrderByDescending(report => report.CreatedAt)
            .ToListAsync(ct);

    public Task<LostReport?> GetByIdWithLineAsync(
        long reportId,
        CancellationToken ct = default)
        => db.LostReports
            .AsNoTracking()
            .Include(report => report.Line)
            .FirstOrDefaultAsync(report => report.ReportId == reportId, ct);

    public Task<LostReport?> GetForCancellationAsync(
        long reportId,
        CancellationToken ct = default)
        => db.LostReports
            .Include(report => report.Matches)
                .ThenInclude(match => match.Item)
            .Include(report => report.Matches)
                .ThenInclude(match => match.Claim)
            .FirstOrDefaultAsync(report => report.ReportId == reportId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
