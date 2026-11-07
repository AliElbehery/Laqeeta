using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public interface IClaimService
{
    Task<OwnershipClaim> SubmitAsync(long matchId, long userId, CancellationToken ct = default);
    Task<OwnershipClaim> VerifyAsync(long claimId, long employeeId, VerifyClaimDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<OwnershipClaim>> GetMineAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyList<OwnershipClaim>> GetForStationAsync(long stationId, ClaimStatus? status = null, CancellationToken ct = default);
    Task<OwnershipClaim> HandOverAsync(long claimId, long employeeId, CancellationToken ct = default);
}
