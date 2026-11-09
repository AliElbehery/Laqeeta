using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class MatchingService(AppDbContext db) : IMatchingService
{
    // Simple rule-based matching:
    //   same category + same color + item found on a station of the line the passenger reported
    //   + item found on/after the lost date.
    public async Task<IReadOnlyList<PossibleMatch>> GenerateForReportAsync(long reportId, CancellationToken ct = default)
    {
        var report = await db.LostReports.FirstOrDefaultAsync(r => r.ReportId == reportId, ct)
                     ?? throw new NotFoundException($"Report {reportId} not found.", Err.ReportNotFound);

        if (report.Status == LostReportStatus.Closed) return Array.Empty<PossibleMatch>();

        var id = report.ReportId;
        var category = report.Category;
        var color = report.Color;
        var lineId = report.LineId;
        var lostFrom = report.LostDate.ToDateTime(TimeOnly.MinValue);

        var items = await db.FoundItems
            .Where(i => i.ItemStatus == FoundItemStatus.InStorage
                        && i.Category == category
                        && i.Color == color
                        && i.FoundAt >= lostFrom
                        && i.FoundStation.StationLines.Any(sl => sl.LineId == lineId)
                        && !db.PossibleMatches.Any(m => m.ReportId == id && m.ItemId == i.ItemId))
            .ToListAsync(ct);

        var created = new List<PossibleMatch>();
        foreach (var item in items)
        {
            var match = new PossibleMatch { Report = report, Item = item };
            created.Add(match);
            db.PossibleMatches.Add(match);
            ServiceHelpers.Notify(db, report.UserId, NotificationType.MatchFound,
                "Possible match for your lost item",
                "An item similar to your report was found. Check your matches.", report, match,
                NotifCode.MatchFoundForReport);
        }

        if (created.Count > 0) await db.SaveChangesAsync(ct);
        return created;
    }

    public async Task<IReadOnlyList<PossibleMatch>> GenerateForItemAsync(long itemId, CancellationToken ct = default)
    {
        var item = await db.FoundItems
            .Include(i => i.FoundStation).ThenInclude(s => s.StationLines)
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct)
            ?? throw new NotFoundException($"Item {itemId} not found.", Err.ItemNotFound);

        if (item.ItemStatus != FoundItemStatus.InStorage) return Array.Empty<PossibleMatch>();

        var id = item.ItemId;
        var category = item.Category;
        var color = item.Color;
        var lineIds = item.FoundStation.StationLines.Select(sl => sl.LineId).ToList();
        var foundDate = DateOnly.FromDateTime(item.FoundAt);

        var reports = await db.LostReports
            .Where(r => r.Status != LostReportStatus.Closed
                        && r.Category == category
                        && r.Color == color
                        && lineIds.Contains(r.LineId)
                        && r.LostDate <= foundDate
                        && !db.PossibleMatches.Any(m => m.ItemId == id && m.ReportId == r.ReportId))
            .ToListAsync(ct);

        var created = new List<PossibleMatch>();
        foreach (var report in reports)
        {
            var match = new PossibleMatch { Report = report, Item = item };
            created.Add(match);
            db.PossibleMatches.Add(match);
            ServiceHelpers.Notify(db, report.UserId, NotificationType.MatchFound,
                "Possible match for your lost item",
                "A newly found item looks like your report. Check your matches.", report, match,
                NotifCode.MatchFoundForItem);
        }

        if (created.Count > 0) await db.SaveChangesAsync(ct);
        return created;
    }

    public async Task<IReadOnlyList<PossibleMatch>> GetForReportAsync(long reportId, long userId,
                                                                      CancellationToken ct = default)
    {
        var report = await db.LostReports.AsNoTracking().FirstOrDefaultAsync(r => r.ReportId == reportId, ct)
                     ?? throw new NotFoundException("Report not found.", Err.ReportNotFound);
        if (report.UserId != userId) throw new ForbiddenException("Not your report.", Err.NotYourReport);

        var matches = await db.PossibleMatches.AsNoTracking()
            .Where(m => m.ReportId == reportId && m.MatchStatus != MatchStatus.Rejected)
            .Include(m => m.Item).ThenInclude(i => i.CurrentStation)
            .ToListAsync(ct);

        // never expose the employee-only verification details to the passenger
        // (safe: entities are untracked, nothing is written back)
        foreach (var m in matches) m.Item.PrivateIdentifyingDetails = null;
        return matches;
    }

    public async Task<IReadOnlyList<PossibleMatch>> GetForStationAsync(long stationId, MatchStatus? status = null,
                                                                       CancellationToken ct = default)
    {
        var q = db.PossibleMatches.AsNoTracking().Where(m => m.Item.CurrentStationId == stationId);
        if (status is not null) q = q.Where(m => m.MatchStatus == status);

        return await q.Include(m => m.Report).ThenInclude(r => r.Line)
                      .Include(m => m.Item).ThenInclude(i => i.FoundStation)
                      .Include(m => m.Item).ThenInclude(i => i.CurrentStation)
                      .OrderByDescending(m => m.CreatedAt)
                      .ToListAsync(ct);
    }

    public async Task ConfirmAsync(long matchId, long employeeId, CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);
        var match = await LoadMatchAsync(matchId, ct);

        EnsureCanHandle(employee, match);
        if (match.MatchStatus != MatchStatus.Suggested)
            throw new BusinessRuleException("Only suggested matches can be confirmed.", Err.ConfirmNeedsSuggested);
        if (match.Report.Status != LostReportStatus.Open)
            throw new BusinessRuleException("This report is not open (it already has a confirmed match, or it is closed).", Err.ReportNotOpen);
        if (match.Item.ItemStatus != FoundItemStatus.InStorage)
            throw new BusinessRuleException("The item is no longer in storage.", Err.ItemNotInStorage);

        match.MatchStatus = MatchStatus.Confirmed;
        match.Item.ItemStatus = FoundItemStatus.Matched;
        match.Report.Status = LostReportStatus.Matched;

        // note: other suggestions for the same item/report are kept. They cannot be confirmed while the item
        // is reserved, and they become usable again if the claim is rejected or the report is cancelled.

        ServiceHelpers.Notify(db, match.Report.UserId, NotificationType.ClaimUpdate,
            "Your item is ready to claim",
            $"Please visit {match.Item.CurrentStation?.StationName ?? "the station"} with your ID to claim it.",
            match.Report, match, NotifCode.MatchConfirmed, match.Item.CurrentStation?.StationName);

        await db.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(long matchId, long employeeId, CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);
        var match = await LoadMatchAsync(matchId, ct);

        EnsureCanHandle(employee, match);
        if (match.MatchStatus != MatchStatus.Suggested)
            throw new BusinessRuleException("Only suggested matches can be rejected.", Err.RejectNeedsSuggested);

        match.MatchStatus = MatchStatus.Rejected;
        await db.SaveChangesAsync(ct);
    }

    private async Task<PossibleMatch> LoadMatchAsync(long matchId, CancellationToken ct)
        => await db.PossibleMatches
               .Include(m => m.Report)
               .Include(m => m.Item).ThenInclude(i => i.CurrentStation)
               .FirstOrDefaultAsync(m => m.MatchId == matchId, ct)
           ?? throw new NotFoundException($"Match {matchId} not found.", Err.MatchNotFound);

    private static void EnsureCanHandle(User employee, PossibleMatch match)
    {
        if (employee.StationId != match.Item.CurrentStationId)
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);
    }
}
