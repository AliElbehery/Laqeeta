using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public sealed class MatchingRepository(AppDbContext db) : IMatchingRepository
{
    public Task<LostReport?> GetReportAsync(long id, CancellationToken ct)
        => db.LostReports.FirstOrDefaultAsync(x => x.ReportId == id, ct);

    public async Task<IReadOnlyList<FoundItem>> GetCandidatesForReportAsync(LostReport report, DateTimeOffset lostFrom, CancellationToken ct)
        => await db.FoundItems.Where(i => i.ItemStatus == FoundItemStatus.InStorage &&
            i.Category == report.Category && i.Color == report.Color && i.FoundAt >= lostFrom &&
            i.FoundStation.StationLines.Any(sl => sl.LineId == report.LineId) &&
            !db.PossibleMatches.Any(m => m.ReportId == report.ReportId && m.ItemId == i.ItemId)).ToListAsync(ct);

    public Task<FoundItem?> GetItemWithStationAsync(long id, CancellationToken ct)
        => db.FoundItems.Include(i => i.FoundStation).ThenInclude(s => s.StationLines)
            .FirstOrDefaultAsync(i => i.ItemId == id, ct);

    public async Task<IReadOnlyList<LostReport>> GetCandidatesForItemAsync(FoundItem item, IReadOnlyCollection<long> lineIds, DateOnly foundDate, CancellationToken ct)
        => await db.LostReports.Where(r => r.Status != LostReportStatus.Closed &&
            r.Category == item.Category && r.Color == item.Color && lineIds.Contains(r.LineId) &&
            r.LostDate <= foundDate &&
            !db.PossibleMatches.Any(m => m.ItemId == item.ItemId && m.ReportId == r.ReportId)).ToListAsync(ct);

    public void AddMatch(PossibleMatch match) => db.PossibleMatches.Add(match);
    public void AddNotification(Notification notification) => db.Notifications.Add(notification);
    public Task<LostReport?> GetReportForUserAsync(long id, CancellationToken ct)
        => db.LostReports.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == id, ct);
    public async Task<IReadOnlyList<PossibleMatch>> GetMatchesForReportAsync(long id, CancellationToken ct)
    {
        var matches = await db.PossibleMatches.AsNoTracking().Where(m => m.ReportId == id && m.MatchStatus != MatchStatus.Rejected)
            .Include(m => m.Item).ThenInclude(i => i.CurrentStation).ToListAsync(ct);
        foreach (var match in matches) match.Item.PrivateIdentifyingDetails = null;
        return matches;
    }
    public async Task<IReadOnlyList<PossibleMatch>> GetMatchesForStationAsync(long stationId, MatchStatus? status, CancellationToken ct)
    {
        var query = db.PossibleMatches.AsNoTracking().Where(m => m.Item.CurrentStationId == stationId);
        if (status is not null) query = query.Where(m => m.MatchStatus == status);
        return await query.Include(m => m.Report).ThenInclude(r => r.Line)
            .Include(m => m.Item).ThenInclude(i => i.FoundStation)
            .Include(m => m.Item).ThenInclude(i => i.CurrentStation)
            .OrderByDescending(m => m.CreatedAt).ToListAsync(ct);
    }
    public Task<User?> GetUserAsync(long id, CancellationToken ct)
        => db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
    public Task<PossibleMatch?> GetMatchForUpdateAsync(long id, CancellationToken ct)
        => db.PossibleMatches.Include(m => m.Report)
            .Include(m => m.Item).ThenInclude(i => i.CurrentStation)
            .FirstOrDefaultAsync(m => m.MatchId == id, ct);
    public void MarkMatchStatusModified(PossibleMatch match)
        => db.Entry(match).Property(m => m.MatchStatus).IsModified = true;
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void ClearTracking() => db.ChangeTracker.Clear();
}