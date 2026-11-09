using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

public class MatchingServiceTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    // ------------------------------------------------------------ GenerateForReport

    [Fact]
    public async Task GenerateForReport_creates_a_match_and_notifies_the_passenger()
    {
        var item = await S.AddItemAsync();           // black wallet held at Sadat (Line 1)
        var report = await S.AddReportAsync();        // black wallet lost on Line 1

        var created = await S.Matching().GenerateForReportAsync(report.ReportId);

        var match = Assert.Single(created);
        Assert.Equal(item.ItemId, match.ItemId);
        Assert.Equal(MatchStatus.Suggested, match.MatchStatus);

        var notifications = await S.Read(db => db.Notifications.Where(n => n.UserId == S.OmarId).ToListAsync());
        var n = Assert.Single(notifications);
        Assert.Equal(NotificationType.MatchFound, n.Type);
        Assert.Equal(match.MatchId, n.MatchId);
        Assert.Equal(report.ReportId, n.ReportId);
    }

    [Fact]
    public async Task GenerateForReport_only_matches_items_with_same_category_color_line_and_date()
    {
        var report = await S.AddReportAsync();        // black wallet, Line 1, lost yesterday

        await S.AddItemAsync(color: "Red");                                             // wrong color
        await S.AddItemAsync(category: ItemCategory.Phone);                             // wrong category
        await S.AddItemAsync(foundStationId: S.ShubraId);                               // found on Line 2 only
        await S.AddItemAsync(foundAt: DateTime.UtcNow.AddDays(-5));                     // found before it was lost
        await S.AddItemAsync(status: FoundItemStatus.Matched);                          // already reserved
        var good = await S.AddItemAsync();                                              // the only real candidate

        var created = await S.Matching().GenerateForReportAsync(report.ReportId);

        Assert.Equal(good.ItemId, Assert.Single(created).ItemId);
    }

    [Fact]
    public async Task GenerateForReport_matches_items_found_on_any_line_the_station_belongs_to()
    {
        // Attaba is on Line 1 and Line 2: a Line 2 report must match an item found there
        var item = await S.AddItemAsync(heldAtId: S.AttabaId);
        var report = await S.AddReportAsync(lineId: S.Line2Id);

        var created = await S.Matching().GenerateForReportAsync(report.ReportId);

        Assert.Equal(item.ItemId, Assert.Single(created).ItemId);
    }

    [Fact]
    public async Task GenerateForReport_does_not_create_the_same_match_twice()
    {
        await S.AddItemAsync();
        var report = await S.AddReportAsync();

        await S.Matching().GenerateForReportAsync(report.ReportId);
        var second = await S.Matching().GenerateForReportAsync(report.ReportId);

        Assert.Empty(second);
        Assert.Equal(1, await S.Read(db => db.PossibleMatches.CountAsync()));
    }

    [Fact]
    public async Task GenerateForReport_returns_nothing_for_a_closed_report()
    {
        await S.AddItemAsync();
        var report = await S.AddReportAsync(status: LostReportStatus.Closed);

        Assert.Empty(await S.Matching().GenerateForReportAsync(report.ReportId));
    }

    [Fact]
    public async Task GenerateForReport_throws_when_the_report_does_not_exist()
        => await Assert.ThrowsAsync<NotFoundException>(() => S.Matching().GenerateForReportAsync(999));

    // ------------------------------------------------------------ GenerateForItem

    [Fact]
    public async Task GenerateForItem_matches_only_open_reports_that_fit()
    {
        var good = await S.AddReportAsync(S.OmarId);
        await S.AddReportAsync(S.SaraId, status: LostReportStatus.Closed);                     // closed
        await S.AddReportAsync(S.SaraId, lineId: S.Line2Id);                                    // other line
        await S.AddReportAsync(S.SaraId, color: "Red");                                         // other color
        await S.AddReportAsync(S.SaraId, lostDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))); // "lost" after it was found
        var item = await S.AddItemAsync();

        var created = await S.Matching().GenerateForItemAsync(item.ItemId);

        var match = Assert.Single(created);
        Assert.Equal(good.ReportId, match.ReportId);
        var n = Assert.Single(await S.Read(db => db.Notifications.Where(x => x.UserId == S.OmarId).ToListAsync()));
        Assert.Equal(NotificationType.MatchFound, n.Type);
    }

    [Fact]
    public async Task GenerateForItem_returns_nothing_when_the_item_is_not_in_storage()
    {
        await S.AddReportAsync();
        var item = await S.AddItemAsync(status: FoundItemStatus.Returned);

        Assert.Empty(await S.Matching().GenerateForItemAsync(item.ItemId));
    }

    // ------------------------------------------------------------ Confirm

    [Fact]
    public async Task Confirm_reserves_the_item_and_report_and_notifies_the_passenger()
    {
        var (report, item, match) = await S.SuggestedMatchAsync();

        await S.Matching().ConfirmAsync(match.MatchId, S.EmpSadatId);

        var loaded = await S.Read(db => db.PossibleMatches.Include(m => m.Item).Include(m => m.Report)
                                          .SingleAsync(m => m.MatchId == match.MatchId));
        Assert.Equal(MatchStatus.Confirmed, loaded.MatchStatus);
        Assert.Equal(FoundItemStatus.Matched, loaded.Item.ItemStatus);
        Assert.Equal(LostReportStatus.Matched, loaded.Report.Status);

        var n = Assert.Single(await S.Read(db => db.Notifications.Where(x => x.UserId == S.OmarId).ToListAsync()));
        Assert.Equal(NotificationType.ClaimUpdate, n.Type);
    }

    [Fact]
    public async Task Confirm_is_forbidden_for_an_employee_of_another_station()
    {
        var (_, _, match) = await S.SuggestedMatchAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Matching().ConfirmAsync(match.MatchId, S.EmpAttabaId));
    }

    [Fact]
    public async Task Confirm_is_forbidden_for_passengers_and_disabled_employees()
    {
        var (_, _, match) = await S.SuggestedMatchAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Matching().ConfirmAsync(match.MatchId, S.OmarId));

        await S.SetActiveAsync(S.EmpSadatId, false);
        await Assert.ThrowsAsync<ForbiddenException>(() => S.Matching().ConfirmAsync(match.MatchId, S.EmpSadatId));
    }

    [Fact]
    public async Task Confirm_throws_when_the_match_does_not_exist()
        => await Assert.ThrowsAsync<NotFoundException>(() => S.Matching().ConfirmAsync(999, S.EmpSadatId));

    [Fact]
    public async Task Confirm_only_works_on_suggested_matches()
    {
        var (_, _, match) = await S.SuggestedMatchAsync();
        await S.Matching().ConfirmAsync(match.MatchId, S.EmpSadatId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Matching().ConfirmAsync(match.MatchId, S.EmpSadatId));
    }

    [Fact]
    public async Task Confirm_cannot_give_one_report_two_items()
    {
        var report = await S.AddReportAsync();
        var item1 = await S.AddItemAsync();
        var item2 = await S.AddItemAsync();
        var m1 = await S.AddMatchAsync(report.ReportId, item1.ItemId);
        var m2 = await S.AddMatchAsync(report.ReportId, item2.ItemId);

        await S.Matching().ConfirmAsync(m1.MatchId, S.EmpSadatId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Matching().ConfirmAsync(m2.MatchId, S.EmpSadatId));
        var second = await S.Read(db => db.PossibleMatches.SingleAsync(m => m.MatchId == m2.MatchId));
        Assert.Equal(MatchStatus.Suggested, second.MatchStatus);   // untouched, can be used later
    }

    [Fact]
    public async Task Confirm_cannot_give_one_item_to_two_reports()
    {
        var item = await S.AddItemAsync();
        var r1 = await S.AddReportAsync(S.OmarId);
        var r2 = await S.AddReportAsync(S.SaraId);
        var m1 = await S.AddMatchAsync(r1.ReportId, item.ItemId);
        var m2 = await S.AddMatchAsync(r2.ReportId, item.ItemId);

        await S.Matching().ConfirmAsync(m1.MatchId, S.EmpSadatId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Matching().ConfirmAsync(m2.MatchId, S.EmpSadatId));
    }

    // ------------------------------------------------------------ Reject

    [Fact]
    public async Task Reject_marks_the_match_rejected_and_leaves_item_and_report_alone()
    {
        var (_, _, match) = await S.SuggestedMatchAsync();

        await S.Matching().RejectAsync(match.MatchId, S.EmpSadatId);

        var loaded = await S.Read(db => db.PossibleMatches.Include(m => m.Item).Include(m => m.Report)
                                          .SingleAsync(m => m.MatchId == match.MatchId));
        Assert.Equal(MatchStatus.Rejected, loaded.MatchStatus);
        Assert.Equal(FoundItemStatus.InStorage, loaded.Item.ItemStatus);
        Assert.Equal(LostReportStatus.Open, loaded.Report.Status);
    }

    [Fact]
    public async Task Reject_only_works_on_suggested_matches()
    {
        var (_, _, match) = await S.ConfirmedMatchAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Matching().RejectAsync(match.MatchId, S.EmpSadatId));
    }

    [Fact]
    public async Task Reject_is_forbidden_for_an_employee_of_another_station()
    {
        var (_, _, match) = await S.SuggestedMatchAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Matching().RejectAsync(match.MatchId, S.EmpAttabaId));
    }

    // ------------------------------------------------------------ queries

    [Fact]
    public async Task GetForReport_hides_private_details_without_changing_the_database()
    {
        var (report, item, match) = await S.SuggestedMatchAsync();

        var matches = await S.Matching().GetForReportAsync(report.ReportId, S.OmarId);

        var m = Assert.Single(matches);
        Assert.Null(m.Item.PrivateIdentifyingDetails);
        var stored = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal("Blue card inside", stored.PrivateIdentifyingDetails);
    }

    [Fact]
    public async Task GetForReport_skips_rejected_matches()
    {
        var report = await S.AddReportAsync();
        var item1 = await S.AddItemAsync();
        var item2 = await S.AddItemAsync();
        await S.AddMatchAsync(report.ReportId, item1.ItemId, MatchStatus.Rejected);
        var kept = await S.AddMatchAsync(report.ReportId, item2.ItemId);

        var matches = await S.Matching().GetForReportAsync(report.ReportId, S.OmarId);

        Assert.Equal(kept.MatchId, Assert.Single(matches).MatchId);
    }

    [Fact]
    public async Task GetForReport_is_forbidden_for_other_passengers()
    {
        var (report, _, _) = await S.SuggestedMatchAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Matching().GetForReportAsync(report.ReportId, S.SaraId));
    }

    [Fact]
    public async Task GetForStation_returns_only_matches_for_items_held_at_that_station_and_status()
    {
        var (_, _, atSadat) = await S.SuggestedMatchAsync();

        var report2 = await S.AddReportAsync(S.SaraId);
        var itemAtAttaba = await S.AddItemAsync(heldAtId: S.AttabaId);
        await S.AddMatchAsync(report2.ReportId, itemAtAttaba.ItemId);

        var sadat = await S.Matching().GetForStationAsync(S.SadatId);
        Assert.Equal(atSadat.MatchId, Assert.Single(sadat).MatchId);

        Assert.Empty(await S.Matching().GetForStationAsync(S.SadatId, MatchStatus.Confirmed));
        Assert.Single(await S.Matching().GetForStationAsync(S.AttabaId, MatchStatus.Suggested));
    }
}
