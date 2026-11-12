//using Laqeeta.Core.DTOs;
//using Laqeeta.Core.Enums;
//using Laqeeta.Core.Exceptions;
//using Laqeeta.Core.Localization;
//using Microsoft.EntityFrameworkCore;

//namespace Laqeeta.Tests;

//public class ClaimServiceTests : IDisposable
//{
//    private readonly Scenario S = new();

//    public void Dispose() => S.Dispose();

//    private static VerifyClaimDto Approve(string? notes = null) => new(true, true, true, notes);

//    private static VerifyClaimDto Reject(string? notes = null) => new(false, true, false, notes);

//    private Task<T> LoadMatchState<T>(long matchId, Func<Laqeeta.Core.Models.PossibleMatch, T> pick)
//        => S.Read(async db => pick(await db.PossibleMatches.Include(m => m.Item).Include(m => m.Report)
//                                           .Include(m => m.Claim).SingleAsync(m => m.MatchId == matchId)));

//    // ------------------------------------------------------------ Submit

//    [Fact]
//    public async Task Submit_creates_a_pending_claim_for_a_confirmed_match()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();

//        var claim = await S.Claims().SubmitAsync(match.MatchId, S.OmarId);

//        Assert.Equal(ClaimStatus.Pending, claim.ClaimStatus);
//        Assert.Equal(S.OmarId, claim.ClaimedBy);
//        Assert.Equal(match.MatchId, claim.MatchId);
//    }

//    [Fact]
//    public async Task Submit_is_forbidden_for_someone_elses_report()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync(S.OmarId);

//        await Assert.ThrowsAsync<ForbiddenException>(() => S.Claims().SubmitAsync(match.MatchId, S.SaraId));
//    }

//    [Fact]
//    public async Task Submit_requires_a_match_confirmed_by_staff()
//    {
//        var (_, _, match) = await S.SuggestedMatchAsync();

//        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => S.Claims().SubmitAsync(match.MatchId, S.OmarId));
//        Assert.Equal(Err.MatchNotConfirmed, ex.Code);
//    }

//    [Fact]
//    public async Task Submit_cannot_be_done_twice_for_the_same_match()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        await S.Claims().SubmitAsync(match.MatchId, S.OmarId);

//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Claims().SubmitAsync(match.MatchId, S.OmarId));
//    }

//    [Fact]
//    public async Task Submit_throws_when_the_match_does_not_exist()
//        => await Assert.ThrowsAsync<NotFoundException>(() => S.Claims().SubmitAsync(999, S.OmarId));

//    // ------------------------------------------------------------ Verify

//    [Theory]
//    [InlineData(true, false)]
//    [InlineData(false, true)]
//    [InlineData(false, false)]
//    public async Task Verify_cannot_approve_before_both_checks_pass(bool idChecked, bool secretPassed)
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

//        await Assert.ThrowsAsync<BusinessRuleException>(() =>
//            S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, new VerifyClaimDto(true, idChecked, secretPassed, null)));
//    }

//    [Fact]
//    public async Task Verify_approve_marks_the_claim_verified_and_notifies_the_passenger()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

//        var result = await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, Approve("ID matches"));

//        Assert.Equal(ClaimStatus.Verified, result.ClaimStatus);
//        var stored = await S.Read(db => db.OwnershipClaims.SingleAsync(c => c.ClaimId == claim.ClaimId));
//        Assert.Equal(S.EmpSadatId, stored.VerifiedBy);
//        Assert.NotNull(stored.VerifiedAt);
//        Assert.True(stored.IdChecked);
//        Assert.True(stored.SecretAnswerPassed);
//        Assert.Equal("ID matches", stored.VerificationNotes);

//        // item and report stay reserved until hand-over
//        Assert.Equal(FoundItemStatus.Matched, await LoadMatchState(match.MatchId, m => m.Item.ItemStatus));
//        Assert.Equal(MatchStatus.Confirmed, await LoadMatchState(match.MatchId, m => m.MatchStatus));

//        var notes = await S.Read(db => db.Notifications.Where(n => n.UserId == S.OmarId).ToListAsync());
//        Assert.Contains(notes, n => n.Type == NotificationType.ClaimUpdate && n.Title == "Claim approved");
//        Assert.Contains(notes, n => n.Code == NotifCode.ClaimApproved);
//    }

//    [Fact]
//    public async Task Verify_reject_releases_the_item_the_report_and_the_match()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

//        var result = await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, Reject("could not answer"));

//        Assert.Equal(ClaimStatus.Rejected, result.ClaimStatus);
//        Assert.Equal(MatchStatus.Rejected, await LoadMatchState(match.MatchId, m => m.MatchStatus));
//        Assert.Equal(FoundItemStatus.InStorage, await LoadMatchState(match.MatchId, m => m.Item.ItemStatus));
//        Assert.Equal(LostReportStatus.Open, await LoadMatchState(match.MatchId, m => m.Report.Status));

//        var notes = await S.Read(db => db.Notifications.Where(n => n.UserId == S.OmarId).ToListAsync());
//        Assert.Contains(notes, n => n.Title == "Claim rejected");
//        Assert.Contains(notes, n => n.Code == NotifCode.ClaimRejected);
//    }

//    [Fact]
//    public async Task After_a_rejected_claim_the_item_can_be_confirmed_for_another_report()
//    {
//        var (_, item, match) = await S.ConfirmedMatchAsync(S.OmarId);
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);
//        await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, Reject());

//        var saraReport = await S.AddReportAsync(S.SaraId);
//        var saraMatch = await S.AddMatchAsync(saraReport.ReportId, item.ItemId);

//        await S.Matching().ConfirmAsync(saraMatch.MatchId, S.EmpSadatId);

//        Assert.Equal(MatchStatus.Confirmed, await LoadMatchState(saraMatch.MatchId, m => m.MatchStatus));
//    }

//    [Fact]
//    public async Task Verify_is_forbidden_for_an_employee_of_another_station()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

//        await Assert.ThrowsAsync<ForbiddenException>(() => S.Claims().VerifyAsync(claim.ClaimId, S.EmpAttabaId, Approve()));
//    }

//    [Fact]
//    public async Task Verify_only_works_on_pending_claims()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);
//        await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, Approve());

//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, Approve()));
//    }

//    [Fact]
//    public async Task Verify_throws_when_the_claim_does_not_exist()
//        => await Assert.ThrowsAsync<NotFoundException>(() => S.Claims().VerifyAsync(999, S.EmpSadatId, Approve()));

//    // ------------------------------------------------------------ HandOver

//    [Fact]
//    public async Task HandOver_returns_the_item_closes_the_report_and_drops_other_suggestions()
//    {
//        var (report, item, match) = await S.ConfirmedMatchAsync(S.OmarId);
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId, ClaimStatus.Verified);

//        // another passenger has a pending suggestion for the same item
//        var saraReport = await S.AddReportAsync(S.SaraId);
//        var saraMatch = await S.AddMatchAsync(saraReport.ReportId, item.ItemId);

//        var result = await S.Claims().HandOverAsync(claim.ClaimId, S.EmpSadatId);

//        Assert.Equal(ClaimStatus.HandedOver, result.ClaimStatus);
//        Assert.NotNull(result.HandedOverAt);

//        Assert.Equal(FoundItemStatus.Returned, await LoadMatchState(match.MatchId, m => m.Item.ItemStatus));
//        Assert.NotNull(await LoadMatchState(match.MatchId, m => m.Item.ReturnedAt));
//        Assert.Equal(LostReportStatus.Closed, await LoadMatchState(match.MatchId, m => m.Report.Status));
//        Assert.Equal(ClosureReason.Returned, await LoadMatchState(match.MatchId, m => m.Report.ClosureReason));
//        Assert.Equal(MatchStatus.Rejected, await LoadMatchState(saraMatch.MatchId, m => m.MatchStatus));

//        var notes = await S.Read(db => db.Notifications.Where(n => n.UserId == S.OmarId).ToListAsync());
//        Assert.Contains(notes, n => n.Type == NotificationType.ItemReturned);
//    }

//    [Fact]
//    public async Task HandOver_requires_a_verified_claim()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId, ClaimStatus.Pending);

//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Claims().HandOverAsync(claim.ClaimId, S.EmpSadatId));
//    }

//    [Fact]
//    public async Task HandOver_is_forbidden_for_an_employee_of_another_station()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId, ClaimStatus.Verified);

//        await Assert.ThrowsAsync<ForbiddenException>(() => S.Claims().HandOverAsync(claim.ClaimId, S.EmpAttabaId));
//    }

//    [Fact]
//    public async Task HandOver_cannot_be_repeated()
//    {
//        var (_, _, match) = await S.ConfirmedMatchAsync();
//        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId, ClaimStatus.Verified);
//        await S.Claims().HandOverAsync(claim.ClaimId, S.EmpSadatId);

//        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Claims().HandOverAsync(claim.ClaimId, S.EmpSadatId));
//    }

//    // ------------------------------------------------------------ queries

//    [Fact]
//    public async Task GetMine_returns_only_my_claims()
//    {
//        var (_, _, m1) = await S.ConfirmedMatchAsync(S.OmarId);
//        var (_, _, m2) = await S.ConfirmedMatchAsync(S.SaraId);
//        var mine = await S.AddClaimAsync(m1.MatchId, S.OmarId);
//        await S.AddClaimAsync(m2.MatchId, S.SaraId);

//        var result = await S.Claims().GetMineAsync(S.OmarId);

//        Assert.Equal(mine.ClaimId, Assert.Single(result).ClaimId);
//    }

//    [Fact]
//    public async Task GetForStation_filters_by_station_and_status_and_includes_the_claimant()
//    {
//        var (_, _, atSadat) = await S.ConfirmedMatchAsync(S.OmarId);
//        await S.AddClaimAsync(atSadat.MatchId, S.OmarId);

//        var report2 = await S.AddReportAsync(S.SaraId, status: LostReportStatus.Matched);
//        var itemAtAttaba = await S.AddItemAsync(heldAtId: S.AttabaId, status: FoundItemStatus.Matched);
//        var match2 = await S.AddMatchAsync(report2.ReportId, itemAtAttaba.ItemId, MatchStatus.Confirmed);
//        await S.AddClaimAsync(match2.MatchId, S.SaraId, ClaimStatus.Verified);

//        var sadat = await S.Claims().GetForStationAsync(S.SadatId);
//        Assert.Equal("omar", Assert.Single(sadat).ClaimedByUser.FullName);

//        Assert.Empty(await S.Claims().GetForStationAsync(S.SadatId, ClaimStatus.Verified));
//        Assert.Single(await S.Claims().GetForStationAsync(S.AttabaId, ClaimStatus.Verified));
//    }
//}