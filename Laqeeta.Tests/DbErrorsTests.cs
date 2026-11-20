using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

/// <summary>
/// "Already exists" is only the right answer for a UNIQUE violation. Before, every DbUpdateException (timeout, deadlock,
/// NOT NULL, ...) was reported as a duplicate match / duplicate claim, which hid the real problem.
/// </summary>
public class DbErrorsTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    /// <summary>A save that fails for a reason that has nothing to do with uniqueness (NOT NULL column).</summary>
    private Notification BrokenNotification() => new()
    {
        UserId = S.OmarId, Type = NotificationType.System, Title = null!, Message = "m"
    };

    [Fact]
    public async Task A_duplicate_username_is_a_unique_violation()
    {
        using var db = S.NewDb();
        var existing = await db.Users.AsNoTracking().FirstAsync();
        db.Users.Add(new User
        {
            FullName = "Dup", Username = existing.Username, Email = "dup@test.com", Phone = "01000000000", PasswordHash = "x"
        });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        Assert.True(DbErrors.IsUniqueViolation(ex));
    }

    [Fact]
    public async Task A_not_null_violation_is_not_a_unique_violation()
    {
        using var db = S.NewDb();
        db.Notifications.Add(BrokenNotification());

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        Assert.False(DbErrors.IsUniqueViolation(ex));
    }

    [Fact]
    public async Task MatchingRepository_lets_other_save_failures_through_unchanged()
    {
        using var db = S.NewDb();
        var repository = new MatchingRepository(db);
        repository.AddNotification(BrokenNotification());

        // not MatchingPairConflictException
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ClaimRepository_reports_a_second_claim_as_ClaimExists_but_lets_other_failures_through()
    {
        var (_, _, match) = await S.ConfirmedMatchAsync(S.OmarId);
        await S.AddClaimAsync(match.MatchId, S.OmarId);

        using (var db = S.NewDb())
        {
            var repository = new ClaimRepository(db);
            repository.AddClaim(new OwnershipClaim { MatchId = match.MatchId, ClaimedBy = S.OmarId });

            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => repository.SaveClaimAsync(CancellationToken.None));
            Assert.Equal(Err.ClaimExists, ex.Code);
        }

        using (var db = S.NewDb())
        {
            var repository = new ClaimRepository(db);
            repository.AddNotification(BrokenNotification());

            await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveClaimAsync(CancellationToken.None));
        }
    }
}
