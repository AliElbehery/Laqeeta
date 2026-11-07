using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class ClaimService(AppDbContext db) : IClaimService
{
    public async Task<OwnershipClaim> SubmitAsync(long matchId, long userId, CancellationToken ct = default)
    {
        var match = await db.PossibleMatches
            .Include(m => m.Report).Include(m => m.Claim)
            .FirstOrDefaultAsync(m => m.MatchId == matchId, ct)
            ?? throw new NotFoundException("Match not found.");

        if (match.Report.UserId != userId) throw new ForbiddenException("This match is not for your report.");
        if (match.MatchStatus != MatchStatus.Confirmed)
            throw new BusinessRuleException("The match must be confirmed by station staff first.");
        if (match.Claim is not null) throw new BusinessRuleException("A claim already exists for this match.");

        var claim = new OwnershipClaim { MatchId = matchId, ClaimedBy = userId };
        db.OwnershipClaims.Add(claim);
        await db.SaveChangesAsync(ct);
        return claim;
    }

    public async Task<OwnershipClaim> VerifyAsync(long claimId, long employeeId, VerifyClaimDto dto,
                                                  CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);
        var claim = await LoadClaimAsync(claimId, ct);

        if (claim.Match.Item.CurrentStationId != employee.StationId)
            throw new ForbiddenException("The item is held at a different station.");
        if (claim.ClaimStatus != ClaimStatus.Pending)
            throw new BusinessRuleException("Only pending claims can be verified.");
        if (dto.Approve && !(dto.IdChecked && dto.SecretAnswerPassed))
            throw new BusinessRuleException("Cannot approve before checking ID and the secret answer.");

        claim.VerifiedBy = employeeId;
        claim.VerifiedAt = DateTime.UtcNow;
        claim.IdChecked = dto.IdChecked;
        claim.SecretAnswerPassed = dto.SecretAnswerPassed;
        claim.VerificationNotes = dto.Notes;
        claim.ClaimStatus = dto.Approve ? ClaimStatus.Verified : ClaimStatus.Rejected;

        if (!dto.Approve)
        {
            // release everything so the item and the report can be matched again
            claim.Match.MatchStatus = MatchStatus.Rejected;
            claim.Match.Item.ItemStatus = FoundItemStatus.InStorage;
            claim.Match.Report.Status = LostReportStatus.Open;
        }

        ServiceHelpers.Notify(db, claim.ClaimedBy, NotificationType.ClaimUpdate,
            dto.Approve ? "Claim approved" : "Claim rejected",
            dto.Approve ? "Your ownership was verified. You can collect your item."
                        : "We could not verify ownership of this item.",
            claim.Match.Report, claim.Match);

        await db.SaveChangesAsync(ct);
        return claim;
    }

    public async Task<OwnershipClaim> HandOverAsync(long claimId, long employeeId, CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);
        var claim = await LoadClaimAsync(claimId, ct);

        if (claim.Match.Item.CurrentStationId != employee.StationId)
            throw new ForbiddenException("The item is held at a different station.");
        if (claim.ClaimStatus != ClaimStatus.Verified)
            throw new BusinessRuleException("Only verified claims can be handed over.");

        var now = DateTime.UtcNow;
        claim.ClaimStatus = ClaimStatus.HandedOver;
        claim.HandedOverAt = now;

        claim.Match.Item.ItemStatus = FoundItemStatus.Returned;
        claim.Match.Item.ReturnedAt = now;

        claim.Match.Report.Status = LostReportStatus.Closed;
        claim.Match.Report.ClosureReason = ClosureReason.Returned;

        // the item is gone: other pending suggestions for it are no longer valid
        var others = await db.PossibleMatches
            .Where(m => m.ItemId == claim.Match.ItemId && m.MatchId != claim.MatchId
                        && m.MatchStatus == MatchStatus.Suggested)
            .ToListAsync(ct);
        foreach (var o in others) o.MatchStatus = MatchStatus.Rejected;

        ServiceHelpers.Notify(db, claim.ClaimedBy, NotificationType.ItemReturned,
            "Item returned", "Your item was handed over. This report is now closed.",
            claim.Match.Report, claim.Match);

        await db.SaveChangesAsync(ct);
        return claim;
    }

    public async Task<IReadOnlyList<OwnershipClaim>> GetMineAsync(long userId, CancellationToken ct = default)
        => await db.OwnershipClaims.AsNoTracking()
               .Where(c => c.ClaimedBy == userId)
               .Include(c => c.Match)
               .OrderByDescending(c => c.ClaimedAt)
               .ToListAsync(ct);

    public async Task<IReadOnlyList<OwnershipClaim>> GetForStationAsync(long stationId, ClaimStatus? status = null,
                                                                        CancellationToken ct = default)
    {
        var q = db.OwnershipClaims.AsNoTracking().Where(c => c.Match.Item.CurrentStationId == stationId);
        if (status is not null) q = q.Where(c => c.ClaimStatus == status);

        return await q.Include(c => c.Match)
                      .Include(c => c.ClaimedByUser)
                      .OrderByDescending(c => c.ClaimedAt)
                      .ToListAsync(ct);
    }

    private async Task<OwnershipClaim> LoadClaimAsync(long claimId, CancellationToken ct)
        => await db.OwnershipClaims
               .Include(c => c.Match).ThenInclude(m => m.Item)
               .Include(c => c.Match).ThenInclude(m => m.Report)
               .FirstOrDefaultAsync(c => c.ClaimId == claimId, ct)
           ?? throw new NotFoundException($"Claim {claimId} not found.");
}
