using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

public interface IClaimRepository
{
    Task<PossibleMatch?> GetMatchForSubmissionAsync(long matchId, CancellationToken ct);
    void AddClaim(OwnershipClaim claim);
    void TouchMatchStatus(PossibleMatch match);
    Task SaveClaimAsync(CancellationToken ct);
    Task<User?> GetUserAsync(long userId, CancellationToken ct);
    Task<OwnershipClaim?> GetClaimForUpdateAsync(long claimId, CancellationToken ct);
    Task<IReadOnlyList<OwnershipClaim>> GetMineAsync(long userId, CancellationToken ct);
    Task<IReadOnlyList<OwnershipClaim>> GetForStationAsync(long stationId, ClaimStatus? status, CancellationToken ct);
    Task<IReadOnlyList<PossibleMatch>> GetOtherSuggestedMatchesAsync(long itemId, long exceptMatchId, CancellationToken ct);
    void AddNotification(Notification notification);
    Task SaveChangesAsync(CancellationToken ct);
}