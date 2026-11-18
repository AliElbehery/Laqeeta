using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public sealed class ClaimRepository(AppDbContext db) : IClaimRepository
{
    public Task<PossibleMatch?> GetMatchForSubmissionAsync(long id, CancellationToken ct)
        => db.PossibleMatches.Include(m => m.Report).Include(m => m.Claim).FirstOrDefaultAsync(m => m.MatchId == id, ct);
    public void AddClaim(OwnershipClaim claim) => db.OwnershipClaims.Add(claim);
    public void TouchMatchStatus(PossibleMatch match) => db.Entry(match).Property(m => m.MatchStatus).IsModified = true;
    public async Task SaveClaimAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        { throw new BusinessRuleException("A claim already exists for this match.", Err.ClaimExists); }
    }
    public Task<User?> GetUserAsync(long id, CancellationToken ct) => db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
    public Task<OwnershipClaim?> GetClaimForUpdateAsync(long id, CancellationToken ct)
        => db.OwnershipClaims.Include(c => c.Match).ThenInclude(m => m.Item)
            .Include(c => c.Match).ThenInclude(m => m.Report).FirstOrDefaultAsync(c => c.ClaimId == id, ct);
    public async Task<IReadOnlyList<OwnershipClaim>> GetMineAsync(long id, CancellationToken ct)
        => await db.OwnershipClaims.AsNoTracking().Where(c => c.ClaimedBy == id).Include(c => c.Match)
            .OrderByDescending(c => c.ClaimedAt).ToListAsync(ct);
    public async Task<IReadOnlyList<OwnershipClaim>> GetForStationAsync(long id, ClaimStatus? status, CancellationToken ct)
    {
        var q = db.OwnershipClaims.AsNoTracking().Where(c => c.Match.Item.CurrentStationId == id);
        if (status is not null) q = q.Where(c => c.ClaimStatus == status);
        return await q.Include(c => c.Match).Include(c => c.ClaimedByUser).OrderByDescending(c => c.ClaimedAt).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<PossibleMatch>> GetOtherSuggestedMatchesAsync(long itemId, long exceptId, CancellationToken ct)
        => await db.PossibleMatches.Where(m => m.ItemId == itemId && m.MatchId != exceptId && m.MatchStatus == MatchStatus.Suggested).ToListAsync(ct);
    public void AddNotification(Notification notification) => db.Notifications.Add(notification);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}