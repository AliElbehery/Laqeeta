using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Services;

public class LostReportService(
    ILostReportRepository repository,
    IMatchingService matching) : ILostReportService
{
    public async Task<LostReport> CreateAsync(
        long userId,
        CreateLostReportDto dto,
        CancellationToken ct = default)
    {
        var user = await repository.GetUserAsync(userId, ct)
                   ?? throw new NotFoundException("User not found.", Err.UserNotFound);

        if (!user.IsActive)
            throw new ForbiddenException("Account is disabled.", Err.AccountDisabled);

        if (!await repository.MetroLineExistsAsync(dto.LineId, ct))
            throw new NotFoundException("Metro line not found.", Err.LineNotFound);

        if (dto.LostDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw new BusinessRuleException("Lost date cannot be in the future.", Err.LostDateFuture);

        var report = new LostReport
        {
            UserId = userId,
            LineId = dto.LineId,
            ContactPhone = dto.ContactPhone,
            Category = dto.Category,
            Color = dto.Color.Trim(),
            LostDate = dto.LostDate,
            ApproximateTime = dto.ApproximateTime,
            Direction = dto.Direction,
            Description = dto.Description
        };

        await repository.AddAsync(report, ct);
        await repository.SaveChangesAsync(ct);

        await matching.GenerateForReportAsync(report.ReportId, ct);
        return report;
    }

    public Task<IReadOnlyList<LostReport>> GetByUserAsync(
        long userId,
        CancellationToken ct = default)
        => repository.GetByUserAsync(userId, ct);

    public async Task<LostReport> GetByIdAsync(
        long reportId,
        long userId,
        CancellationToken ct = default)
    {
        var report = await repository.GetByIdWithLineAsync(reportId, ct)
                     ?? throw new NotFoundException("Report not found.", Err.ReportNotFound);

        if (report.UserId != userId)
            throw new ForbiddenException("Not your report.", Err.NotYourReport);

        return report;
    }

    public async Task CancelAsync(
        long reportId,
        long userId,
        CancellationToken ct = default)
    {
        var report = await repository.GetForCancellationAsync(reportId, ct)
                     ?? throw new NotFoundException("Report not found.", Err.ReportNotFound);

        if (report.UserId != userId)
            throw new ForbiddenException("Not your report.", Err.NotYourReport);

        if (report.Status == LostReportStatus.Closed)
            throw new BusinessRuleException("Report is already closed.", Err.ReportAlreadyClosed);

        report.Status = LostReportStatus.Closed;
        report.ClosureReason = ClosureReason.CancelledByUser;

        foreach (var match in report.Matches.Where(
                     match => match.MatchStatus is MatchStatus.Suggested or MatchStatus.Confirmed))
        {
            if (match.MatchStatus == MatchStatus.Confirmed)
            {
                // Release the reserved item and cancel its open claim.
                if (match.Item.ItemStatus == FoundItemStatus.Matched)
                    match.Item.ItemStatus = FoundItemStatus.InStorage;

                if (match.Claim is { ClaimStatus: ClaimStatus.Pending or ClaimStatus.Verified })
                    match.Claim.ClaimStatus = ClaimStatus.Rejected;
            }

            match.MatchStatus = MatchStatus.Rejected;
        }

        await repository.SaveChangesAsync(ct);
    }
}