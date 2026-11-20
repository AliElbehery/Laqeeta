using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Core.Time;

namespace Laqeeta.Application.Services;

public sealed class MatchingService(IMatchingRepository repository) : IMatchingService
{
    public Task<IReadOnlyList<PossibleMatch>> GenerateForReportAsync(long reportId, CancellationToken ct = default)
        => RetryOnceIfPairAlreadyExistsAsync(() => GenerateForReportCoreAsync(reportId, ct));

    public Task<IReadOnlyList<PossibleMatch>> GenerateForItemAsync(long itemId, CancellationToken ct = default)
        => RetryOnceIfPairAlreadyExistsAsync(() => GenerateForItemCoreAsync(itemId, ct));

    private async Task<IReadOnlyList<PossibleMatch>> RetryOnceIfPairAlreadyExistsAsync(
        Func<Task<IReadOnlyList<PossibleMatch>>> generate)
    {
        try { return await generate(); }
        catch (MatchingPairConflictException)
        {
            repository.ClearTracking();
            return await generate();
        }
    }

    private async Task<IReadOnlyList<PossibleMatch>> GenerateForReportCoreAsync(long reportId, CancellationToken ct)
    {
        var report = await repository.GetReportAsync(reportId, ct)
            ?? throw new NotFoundException($"Report {reportId} not found.", Err.ReportNotFound);
        if (report.Status == LostReportStatus.Closed) return Array.Empty<PossibleMatch>();

        // the lost day starts at 00:00 Cairo (01:00 on the day the clocks go forward): never throws
        var lostFrom = EgyptTime.StartOfDayUtc(report.LostDate);
        var items = await repository.GetCandidatesForReportAsync(report, lostFrom, ct);
        var created = new List<PossibleMatch>();
        foreach (var item in items)
        {
            var match = new PossibleMatch { Report = report, Item = item };
            created.Add(match);
            repository.AddMatch(match);
            repository.AddNotification(BuildNotification(report.UserId, NotificationType.MatchFound,
                "Possible match for your lost item", "An item similar to your report was found. Check your matches.",
                report, match, NotifCode.MatchFoundForReport));
        }
        if (created.Count > 0) await repository.SaveChangesAsync(ct);
        return created;
    }

    private async Task<IReadOnlyList<PossibleMatch>> GenerateForItemCoreAsync(long itemId, CancellationToken ct)
    {
        var item = await repository.GetItemWithStationAsync(itemId, ct)
            ?? throw new NotFoundException($"Item {itemId} not found.", Err.ItemNotFound);
        if (item.ItemStatus != FoundItemStatus.InStorage) return Array.Empty<PossibleMatch>();
        var lineIds = item.FoundStation.StationLines.Select(x => x.LineId).ToArray();
        var foundDate = EgyptTime.DateOf(item.FoundAt);
        var reports = await repository.GetCandidatesForItemAsync(item, lineIds, foundDate, ct);
        var created = new List<PossibleMatch>();
        foreach (var report in reports)
        {
            var match = new PossibleMatch { Report = report, Item = item };
            created.Add(match);
            repository.AddMatch(match);
            repository.AddNotification(BuildNotification(report.UserId, NotificationType.MatchFound,
                "Possible match for your lost item", "A newly found item looks like your report. Check your matches.",
                report, match, NotifCode.MatchFoundForItem));
        }
        if (created.Count > 0) await repository.SaveChangesAsync(ct);
        return created;
    }

    public async Task<IReadOnlyList<PossibleMatch>> GetForReportAsync(long reportId, long userId, CancellationToken ct = default)
    {
        var report = await repository.GetReportForUserAsync(reportId, ct)
            ?? throw new NotFoundException("Report not found.", Err.ReportNotFound);
        if (report.UserId != userId) throw new ForbiddenException("Not your report.", Err.NotYourReport);
        return await repository.GetMatchesForReportAsync(reportId, ct);
    }

    public Task<IReadOnlyList<PossibleMatch>> GetForStationAsync(long stationId, MatchStatus? status = null, CancellationToken ct = default)
        => repository.GetMatchesForStationAsync(stationId, status, ct);

    public async Task ConfirmAsync(long matchId, long employeeId, CancellationToken ct = default)
    {
        var employee = await RequireEmployeeAsync(employeeId, ct);
        var match = await repository.GetMatchForUpdateAsync(matchId, ct)
            ?? throw new NotFoundException($"Match {matchId} not found.", Err.MatchNotFound);
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
        repository.AddNotification(BuildNotification(match.Report.UserId, NotificationType.ClaimUpdate,
            "Your item is ready to claim",
            $"Please visit {match.Item.CurrentStation?.StationName ?? "the station"} with your ID to claim it.",
            match.Report, match, NotifCode.MatchConfirmed, match.Item.CurrentStation?.StationName));
        await repository.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(long matchId, long employeeId, CancellationToken ct = default)
    {
        var employee = await RequireEmployeeAsync(employeeId, ct);
        var match = await repository.GetMatchForUpdateAsync(matchId, ct)
            ?? throw new NotFoundException($"Match {matchId} not found.", Err.MatchNotFound);
        EnsureCanHandle(employee, match);
        if (match.MatchStatus != MatchStatus.Suggested)
            throw new BusinessRuleException("Only suggested matches can be rejected.", Err.RejectNeedsSuggested);
        match.MatchStatus = MatchStatus.Rejected;
        await repository.SaveChangesAsync(ct);
    }

    private async Task<User> RequireEmployeeAsync(long id, CancellationToken ct)
    {
        var user = await repository.GetUserAsync(id, ct)
            ?? throw new NotFoundException($"User {id} not found.", Err.UserNotFound);
        if (!user.IsActive || user.Role != UserRole.Employee || user.StationId is null)
            throw new ForbiddenException("This action requires an active employee assigned to a station.", Err.EmployeeRequired);
        return user;
    }

    private static void EnsureCanHandle(User employee, PossibleMatch match)
    {
        if (employee.StationId != match.Item.CurrentStationId)
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);
    }

    private static Notification BuildNotification(long userId, NotificationType type, string title, string message,
        LostReport report, PossibleMatch match, string code, string? param = null)
        => new() { UserId = userId, Type = type, Title = title, Message = message,
            Report = report, Match = match, Code = code, Param = param };
}