//using Laqeeta.Core.DTOs;
//using Laqeeta.Core.Enums;
//using Laqeeta.Core.Exceptions;
//using Laqeeta.Core.Localization;
//using Laqeeta.Infrastructure.Services;
//using Microsoft.EntityFrameworkCore;

//namespace Laqeeta.Tests;

///// <summary>
///// Two requests that touch the same rows at the same moment. The service under test runs on a context whose
///// <see cref="SqliteAppDbContext.BeforeNextSave"/> lets a competing request finish first - after our service has
///// already read its data and decided what to do, which a normal one-call-after-another test can never reproduce.
///// </summary>
//public class ConcurrencyTests : IDisposable
//{
//    private readonly Scenario S = new();

//    public void Dispose() => S.Dispose();

//    private Task<MatchStatus> MatchStatusOf(long matchId)
//        => S.Read(db => db.PossibleMatches.Where(m => m.MatchId == matchId).Select(m => m.MatchStatus).SingleAsync());

//    [Fact]
//    public async Task Every_save_gives_the_row_a_new_concurrency_stamp()
//    {
//        var item = await S.AddItemAsync();
//        var before = await S.Read(db => db.FoundItems.Where(i => i.ItemId == item.ItemId)
//                                          .Select(i => i.ConcurrencyStamp).SingleAsync());

//        await S.Items().TransferAsync(item.ItemId, S.AttabaId, S.EmpSadatId);

//        var after = await S.Read(db => db.FoundItems.Where(i => i.ItemId == item.ItemId)
//                                         .Select(i => i.ConcurrencyStamp).SingleAsync());
//        Assert.NotEqual(Guid.Empty, before);
//        Assert.NotEqual(before, after);
//    }

//    [Fact]
//    public async Task Two_employees_cannot_reserve_the_same_item_at_the_same_time()
//    {
//        var item = await S.AddItemAsync();
//        var r1 = await S.AddReportAsync(S.OmarId);
//        var r2 = await S.AddReportAsync(S.SaraId);
//        var m1 = await S.AddMatchAsync(r1.ReportId, item.ItemId);
//        var m2 = await S.AddMatchAsync(r2.ReportId, item.ItemId);

//        using var slowRequest = S.NewDb();
//        slowRequest.BeforeNextSave = () => S.Matching().ConfirmAsync(m2.MatchId, S.EmpSadatId);   // the other employee wins

//        var ex = await Assert.ThrowsAsync<ConflictException>(
//            () => new MatchingService(slowRequest).ConfirmAsync(m1.MatchId, S.EmpSadatId));

//        Assert.Equal(Err.ConcurrencyConflict, ex.Code);
//        Assert.Equal(MatchStatus.Confirmed, await MatchStatusOf(m2.MatchId));
//        Assert.Equal(MatchStatus.Suggested, await MatchStatusOf(m1.MatchId));    // nothing of the loser was saved
//        Assert.Equal(LostReportStatus.Open, await S.Read(db => db.LostReports
//            .Where(r => r.ReportId == r1.ReportId).Select(r => r.Status).SingleAsync()));
//    }

//    [Fact]
//    public async Task A_claim_is_not_created_for_a_match_whose_report_was_cancelled_in_the_meantime()
//    {
//        var (report, _, match) = await S.ConfirmedMatchAsync(S.OmarId);

//        using var slowRequest = S.NewDb();
//        slowRequest.BeforeNextSave = () => S.Reports().CancelAsync(report.ReportId, S.OmarId);

//        await Assert.ThrowsAsync<ConflictException>(
//            () => new ClaimService(slowRequest).SubmitAsync(match.MatchId, S.OmarId));

//        Assert.Equal(0, await S.Read(db => db.OwnershipClaims.CountAsync()));
//        Assert.Equal(MatchStatus.Rejected, await MatchStatusOf(match.MatchId));
//    }

//    [Fact]
//    public async Task Two_employees_cannot_verify_the_same_claim_at_the_same_time()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync(S.OmarId);
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

//        using var slowRequest = S.NewDb();
//        slowRequest.BeforeNextSave = () => S.Claims().VerifyAsync(
//            claim.ClaimId, S.EmpSadatId, new VerifyClaimDto(false, true, false, "rejected first"));

//        await Assert.ThrowsAsync<ConflictException>(() => new ClaimService(slowRequest).VerifyAsync(
//            claim.ClaimId, S.EmpSadatId, new VerifyClaimDto(true, true, true, null)));

//        var saved = await S.Read(db => db.OwnershipClaims.SingleAsync(c => c.ClaimId == claim.ClaimId));
//        Assert.Equal(ClaimStatus.Rejected, saved.ClaimStatus);                  // the first decision stands
//        Assert.Equal("rejected first", saved.VerificationNotes);
//        Assert.Equal(1, await S.Read(db => db.Notifications.CountAsync(n => n.UserId == S.OmarId)));   // no "approved" on top
//    }

//    [Fact]
//    public async Task Matching_a_report_and_an_item_at_the_same_moment_creates_the_match_once_without_an_error()
//    {
//        var report = await S.AddReportAsync(S.OmarId);
//        var item = await S.AddItemAsync();

//        using var slowRequest = S.NewDb();
//        slowRequest.BeforeNextSave = () => S.Matching().GenerateForReportAsync(report.ReportId);   // creates the pair first

//        var created = await new MatchingService(slowRequest).GenerateForItemAsync(item.ItemId);    // must not throw

//        Assert.Empty(created);
//        Assert.Equal(1, await S.Read(db => db.PossibleMatches.CountAsync()));
//        Assert.Equal(1, await S.Read(db => db.Notifications.CountAsync(n => n.UserId == S.OmarId)));
//    }
//}