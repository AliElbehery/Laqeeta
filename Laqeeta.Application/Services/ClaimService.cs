using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Services;

public sealed class ClaimService(IClaimRepository repository) : IClaimService
{
    public async Task<OwnershipClaim> SubmitAsync(long matchId, long userId, CancellationToken ct = default)
    {
        var match = await repository.GetMatchForSubmissionAsync(matchId, ct)
            ?? throw new NotFoundException("Match not found.", Err.MatchNotFound);
        if (match.Report.UserId != userId) throw new ForbiddenException("This match is not for your report.", Err.NotYourMatch);
        if (match.MatchStatus != MatchStatus.Confirmed)
            throw new BusinessRuleException("The match must be confirmed by station staff first.", Err.MatchNotConfirmed);
        if (match.Claim is not null) throw new BusinessRuleException("A claim already exists for this match.", Err.ClaimExists);
        var claim = new OwnershipClaim { MatchId = matchId, ClaimedBy = userId };
        repository.AddClaim(claim);
        repository.TouchMatchStatus(match);
        await repository.SaveClaimAsync(ct);
        return claim;
    }

    public async Task<OwnershipClaim> VerifyAsync(long claimId, long employeeId, VerifyClaimDto dto, CancellationToken ct = default)
    {
        var employee = await RequireEmployeeAsync(employeeId, ct);
        var claim = await LoadClaimAsync(claimId, ct);
        EnsureStation(employee, claim);
        if (claim.ClaimStatus != ClaimStatus.Pending)
            throw new BusinessRuleException("Only pending claims can be verified.", Err.ClaimNotPending);
        if (dto.Approve && !(dto.IdChecked && dto.SecretAnswerPassed))
            throw new BusinessRuleException("Cannot approve before checking ID and the secret answer.", Err.ApproveNeedsChecks);
        claim.VerifiedBy = employeeId;
        claim.VerifiedAt = DateTime.UtcNow;
        claim.IdChecked = dto.IdChecked;
        claim.SecretAnswerPassed = dto.SecretAnswerPassed;
        claim.VerificationNotes = dto.Notes;
        claim.ClaimStatus = dto.Approve ? ClaimStatus.Verified : ClaimStatus.Rejected;
        if (!dto.Approve)
        {
            claim.Match.MatchStatus = MatchStatus.Rejected;
            claim.Match.Item.ItemStatus = FoundItemStatus.InStorage;
            claim.Match.Report.Status = LostReportStatus.Open;
        }
        repository.AddNotification(BuildNotification(claim.ClaimedBy, NotificationType.ClaimUpdate,
            dto.Approve ? "Claim approved" : "Claim rejected",
            dto.Approve ? "Your ownership was verified. You can collect your item." : "We could not verify ownership of this item.",
            claim.Match.Report, claim.Match, dto.Approve ? NotifCode.ClaimApproved : NotifCode.ClaimRejected));
        await repository.SaveChangesAsync(ct);
        return claim;
    }

    public Task<IReadOnlyList<OwnershipClaim>> GetMineAsync(long userId, CancellationToken ct = default)
        => repository.GetMineAsync(userId, ct);

    public Task<IReadOnlyList<OwnershipClaim>> GetForStationAsync(long stationId, ClaimStatus? status = null, CancellationToken ct = default)
        => repository.GetForStationAsync(stationId, status, ct);

    public async Task<OwnershipClaim> HandOverAsync(long claimId, long employeeId, CancellationToken ct = default)
    {
        var employee = await RequireEmployeeAsync(employeeId, ct);
        var claim = await LoadClaimAsync(claimId, ct);
        EnsureStation(employee, claim);
        if (claim.ClaimStatus != ClaimStatus.Verified)
            throw new BusinessRuleException("Only verified claims can be handed over.", Err.ClaimNotVerified);
        var now = DateTime.UtcNow;
        claim.ClaimStatus = ClaimStatus.HandedOver;
        claim.HandedOverAt = now;
        claim.Match.Item.ItemStatus = FoundItemStatus.Returned;
        claim.Match.Item.ReturnedAt = now;
        claim.Match.Report.Status = LostReportStatus.Closed;
        claim.Match.Report.ClosureReason = ClosureReason.Returned;
        var others = await repository.GetOtherSuggestedMatchesAsync(claim.Match.ItemId, claim.MatchId, ct);
        foreach (var other in others) other.MatchStatus = MatchStatus.Rejected;
        repository.AddNotification(BuildNotification(claim.ClaimedBy, NotificationType.ItemReturned,
            "Item returned", "Your item was handed over. This report is now closed.",
            claim.Match.Report, claim.Match, NotifCode.ItemReturned));
        await repository.SaveChangesAsync(ct);
        return claim;
    }

    private async Task<OwnershipClaim> LoadClaimAsync(long id, CancellationToken ct)
        => await repository.GetClaimForUpdateAsync(id, ct) ?? throw new NotFoundException($"Claim {id} not found.", Err.ClaimNotFound);

    private async Task<User> RequireEmployeeAsync(long id, CancellationToken ct)
    {
        var user = await repository.GetUserAsync(id, ct) ?? throw new NotFoundException($"User {id} not found.", Err.UserNotFound);
        if (!user.IsActive || user.Role != UserRole.Employee || user.StationId is null)
            throw new ForbiddenException("This action requires an active employee assigned to a station.", Err.EmployeeRequired);
        return user;
    }

    private static void EnsureStation(User employee, OwnershipClaim claim)
    {
        if (claim.Match.Item.CurrentStationId != employee.StationId)
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);
    }

    private static Notification BuildNotification(long userId, NotificationType type, string title, string message,
        LostReport report, PossibleMatch match, string code)
        => new() { UserId = userId, Type = type, Title = title, Message = message, Report = report, Match = match, Code = code };
}