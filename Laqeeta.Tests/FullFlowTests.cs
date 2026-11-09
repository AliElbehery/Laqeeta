using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

/// <summary>The whole story from the passenger's report to the item being handed over.</summary>
public class FullFlowTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    [Fact]
    public async Task Report_then_found_item_then_confirm_claim_verify_and_hand_over()
    {
        // 1. Omar reports a black wallet lost on Line 1
        var report = await S.Reports().CreateAsync(S.OmarId, new CreateLostReportDto(
            S.Line1Id, "01000000001", ItemCategory.Wallet, "Black",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), null, null, "Black leather wallet"));

        // 2. an employee at Sadat logs a black wallet -> the system suggests the match
        var item = await S.Items().LogAsync(S.EmpSadatId, new CreateFoundItemDto(
            S.SadatId, ItemCategory.Wallet, "Black", "Black wallet", "Photo of a child inside", DateTime.UtcNow.AddMinutes(-30)));

        var matches = await S.Matching().GetForReportAsync(report.ReportId, S.OmarId);
        var match = Assert.Single(matches);
        Assert.Equal(item.ItemId, match.ItemId);
        Assert.Null(match.Item.PrivateIdentifyingDetails);               // passenger never sees it

        // 3. the employee confirms, Omar claims, the employee verifies and hands over
        await S.Matching().ConfirmAsync(match.MatchId, S.EmpSadatId);
        var claim = await S.Claims().SubmitAsync(match.MatchId, S.OmarId);
        await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, new VerifyClaimDto(true, true, true, "ID OK"));
        await S.Claims().HandOverAsync(claim.ClaimId, S.EmpSadatId);

        // 4. final state
        var finalReport = await S.Read(db => db.LostReports.SingleAsync(r => r.ReportId == report.ReportId));
        Assert.Equal(LostReportStatus.Closed, finalReport.Status);
        Assert.Equal(ClosureReason.Returned, finalReport.ClosureReason);

        var finalItem = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal(FoundItemStatus.Returned, finalItem.ItemStatus);
        Assert.NotNull(finalItem.ReturnedAt);

        var notes = await S.Read(db => db.Notifications.Where(n => n.UserId == S.OmarId).ToListAsync());
        Assert.Equal(1, notes.Count(n => n.Type == NotificationType.MatchFound));
        Assert.Equal(2, notes.Count(n => n.Type == NotificationType.ClaimUpdate));   // "ready to claim" + "approved"
        Assert.Equal(1, notes.Count(n => n.Type == NotificationType.ItemReturned));
    }

    [Fact]
    public async Task A_rejected_claim_puts_everything_back_so_the_real_owner_can_still_get_the_item()
    {
        var first = await S.AddReportAsync(S.OmarId);
        var owner = await S.AddReportAsync(S.SaraId);
        var item = await S.AddItemAsync();
        var firstMatch = await S.AddMatchAsync(first.ReportId, item.ItemId);
        var ownerMatch = await S.AddMatchAsync(owner.ReportId, item.ItemId);

        // The first claimant is confirmed first, but fails verification
        await S.Matching().ConfirmAsync(firstMatch.MatchId, S.EmpSadatId);
        var claim = await S.Claims().SubmitAsync(firstMatch.MatchId, S.OmarId);
        await S.Claims().VerifyAsync(claim.ClaimId, S.EmpSadatId, new VerifyClaimDto(false, true, false, "wrong answers"));

        // The real owner can now be confirmed and receive the item
        await S.Matching().ConfirmAsync(ownerMatch.MatchId, S.EmpSadatId);
        var ownerClaim = await S.Claims().SubmitAsync(ownerMatch.MatchId, S.SaraId);
        await S.Claims().VerifyAsync(ownerClaim.ClaimId, S.EmpSadatId, new VerifyClaimDto(true, true, true, null));
        await S.Claims().HandOverAsync(ownerClaim.ClaimId, S.EmpSadatId);

        var finalItem = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal(FoundItemStatus.Returned, finalItem.ItemStatus);
        var firstReport = await S.Read(db => db.LostReports.SingleAsync(r => r.ReportId == first.ReportId));
        Assert.Equal(LostReportStatus.Open, firstReport.Status);          // the first report is simply open again
    }
}
