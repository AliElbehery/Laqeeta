using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class LostReportService(AppDbContext db, IMatchingService matching) : ILostReportService
{
    public async Task<LostReport> CreateAsync(long userId, CreateLostReportDto dto, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct)
                   ?? throw new NotFoundException("User not found.");
        if (!user.IsActive) throw new ForbiddenException("Account is disabled.");

        if (!await db.MetroLines.AnyAsync(l => l.LineId == dto.LineId, ct))
            throw new NotFoundException("Metro line not found.");

        if (dto.LostDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            throw new BusinessRuleException("Lost date cannot be in the future.");

        var report = new LostReport
        {
            UserId = userId, LineId = dto.LineId, ContactPhone = dto.ContactPhone,
            Category = dto.Category, Color = dto.Color.Trim(), LostDate = dto.LostDate,
            ApproximateTime = dto.ApproximateTime, Direction = dto.Direction,
            Description = dto.Description
        };
        db.LostReports.Add(report);
        await db.SaveChangesAsync(ct);

        await matching.GenerateForReportAsync(report.ReportId, ct);
        return report;
    }

    public async Task<IReadOnlyList<LostReport>> GetByUserAsync(long userId, CancellationToken ct = default)
        => await db.LostReports.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Include(r => r.Line)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<LostReport> GetByIdAsync(long reportId, long userId, CancellationToken ct = default)
    {
        var report = await db.LostReports.AsNoTracking()
            .Include(r => r.Line)
            .FirstOrDefaultAsync(r => r.ReportId == reportId, ct)
            ?? throw new NotFoundException("Report not found.");

        if (report.UserId != userId) throw new ForbiddenException("Not your report.");
        return report;
    }

    public async Task CancelAsync(long reportId, long userId, CancellationToken ct = default)
    {
        var report = await db.LostReports
            .Include(r => r.Matches).ThenInclude(m => m.Item)
            .Include(r => r.Matches).ThenInclude(m => m.Claim)
            .FirstOrDefaultAsync(r => r.ReportId == reportId, ct)
            ?? throw new NotFoundException("Report not found.");

        if (report.UserId != userId) throw new ForbiddenException("Not your report.");
        if (report.Status == LostReportStatus.Closed) throw new BusinessRuleException("Report is already closed.");

        report.Status = LostReportStatus.Closed;
        report.ClosureReason = ClosureReason.CancelledByUser;
        foreach (var m in report.Matches.Where(m => m.MatchStatus is MatchStatus.Suggested or MatchStatus.Confirmed))
        {
            if (m.MatchStatus == MatchStatus.Confirmed)
            {
                // release the reserved item and cancel its open claim
                if (m.Item.ItemStatus == FoundItemStatus.Matched) m.Item.ItemStatus = FoundItemStatus.InStorage;
                if (m.Claim is { ClaimStatus: ClaimStatus.Pending or ClaimStatus.Verified })
                    m.Claim.ClaimStatus = ClaimStatus.Rejected;
            }
            m.MatchStatus = MatchStatus.Rejected;
        }

        await db.SaveChangesAsync(ct);
    }
}
