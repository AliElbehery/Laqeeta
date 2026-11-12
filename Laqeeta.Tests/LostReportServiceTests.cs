using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

public class LostReportServiceTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    private CreateLostReportDto Dto(long? lineId = null, string color = "Black", DateOnly? lostDate = null)
        => new(lineId ?? S.Line1Id, "01000000001", ItemCategory.Wallet, color,
               lostDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), null, "Towards Helwan", "Black leather wallet");

    // ------------------------------------------------------------ Create

    [Fact]
    public async Task Create_saves_an_open_report_and_trims_the_color()
    {
        var report = await S.Reports().CreateAsync(S.OmarId, Dto(color: "  Black "));

        var stored = await S.Read(db => db.LostReports.SingleAsync(r => r.ReportId == report.ReportId));
        Assert.Equal(LostReportStatus.Open, stored.Status);
        Assert.Equal("Black", stored.Color);
        Assert.Equal(S.OmarId, stored.UserId);
    }

    [Fact]
    public async Task Create_immediately_looks_for_matches_among_stored_items()
    {
        var item = await S.AddItemAsync();

        var report = await S.Reports().CreateAsync(S.OmarId, Dto());

        var match = await S.Read(db => db.PossibleMatches.SingleAsync(m => m.ReportId == report.ReportId));
        Assert.Equal(item.ItemId, match.ItemId);
        Assert.Equal(1, await S.Read(db => db.Notifications.CountAsync(n => n.UserId == S.OmarId)));
    }

    [Fact]
    public async Task Create_rejects_an_unknown_line()
        => await Assert.ThrowsAsync<NotFoundException>(() => S.Reports().CreateAsync(S.OmarId, Dto(lineId: 999)));

    [Fact]
    public async Task Create_rejects_a_future_lost_date()
        => await Assert.ThrowsAsync<BusinessRuleException>(() =>
            S.Reports().CreateAsync(S.OmarId, Dto(lostDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)))));

    [Fact]
    public async Task Create_rejects_disabled_and_unknown_users()
    {
        await S.SetActiveAsync(S.OmarId, false);
        await Assert.ThrowsAsync<ForbiddenException>(() => S.Reports().CreateAsync(S.OmarId, Dto()));
        await Assert.ThrowsAsync<NotFoundException>(() => S.Reports().CreateAsync(999, Dto()));
    }

    // ------------------------------------------------------------ Read

    [Fact]
    public async Task GetByUser_returns_only_my_reports()
    {
        var mine = await S.AddReportAsync(S.OmarId);
        await S.AddReportAsync(S.SaraId);

        var result = await S.Reports().GetByUserAsync(S.OmarId);

        Assert.Equal(mine.ReportId, Assert.Single(result).ReportId);
    }

    [Fact]
    public async Task GetPageByUser_continues_past_tied_timestamps_and_only_returns_my_reports()
    {
        var createdAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddTicks(7);
        var tiedReportIds = new List<long>();
        for (var i = 0; i < 12; i++)
            tiedReportIds.Add((await S.AddReportAsync(S.OmarId, createdAt: createdAt)).ReportId);
        var older = await S.AddReportAsync(S.OmarId, createdAt: createdAt.AddTicks(-1));
        await S.AddReportAsync(S.SaraId, createdAt: createdAt);
        await S.AddReportAsync(S.SaraId, createdAt: older.CreatedAt);

        tiedReportIds.Reverse();
        var (first, cursor) = await S.Reports().GetPageByUserAsync(S.OmarId);
        Assert.Equal(tiedReportIds.Take(10), first.Select(report => report.ReportId));
        Assert.NotNull(cursor);

        var (second, nextCursor) = await S.Reports().GetPageByUserAsync(S.OmarId, cursor);
        Assert.Equal(tiedReportIds.Skip(10).Append(older.ReportId), second.Select(report => report.ReportId));
        Assert.Null(nextCursor);
    }

    [Fact]
    public async Task GetPageByUser_returns_no_cursor_when_the_last_page_is_full()
    {
        for (var i = 0; i < 10; i++)
            await S.AddReportAsync(S.OmarId);
        await S.AddReportAsync(S.SaraId);

        var (items, cursor) = await S.Reports().GetPageByUserAsync(S.OmarId);

        Assert.Equal(10, items.Count);
        Assert.Null(cursor);
    }

    [Fact]
    public async Task GetById_is_forbidden_for_other_passengers_and_includes_the_line()
    {
        var report = await S.AddReportAsync(S.OmarId);

        var found = await S.Reports().GetByIdAsync(report.ReportId, S.OmarId);
        Assert.Equal("Line 1", found.Line.LineName);

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Reports().GetByIdAsync(report.ReportId, S.SaraId));
        await Assert.ThrowsAsync<NotFoundException>(() => S.Reports().GetByIdAsync(999, S.OmarId));
    }

    // ------------------------------------------------------------ Cancel

    [Fact]
    public async Task Cancel_closes_the_report_and_rejects_suggested_matches()
    {
        var (report, _, match) = await S.SuggestedMatchAsync();

        await S.Reports().CancelAsync(report.ReportId, S.OmarId);

        var stored = await S.Read(db => db.LostReports.SingleAsync(r => r.ReportId == report.ReportId));
        Assert.Equal(LostReportStatus.Closed, stored.Status);
        Assert.Equal(ClosureReason.CancelledByUser, stored.ClosureReason);
        var m = await S.Read(db => db.PossibleMatches.SingleAsync(x => x.MatchId == match.MatchId));
        Assert.Equal(MatchStatus.Rejected, m.MatchStatus);
    }

    [Fact]
    public async Task Cancel_releases_an_item_that_was_reserved_for_the_report()
    {
        var (report, item, match) = await S.ConfirmedMatchAsync();
        var claim = await S.AddClaimAsync(match.MatchId, S.OmarId);

        await S.Reports().CancelAsync(report.ReportId, S.OmarId);

        var storedItem = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal(FoundItemStatus.InStorage, storedItem.ItemStatus);
        var storedClaim = await S.Read(db => db.OwnershipClaims.SingleAsync(c => c.ClaimId == claim.ClaimId));
        Assert.Equal(ClaimStatus.Rejected, storedClaim.ClaimStatus);
        var storedMatch = await S.Read(db => db.PossibleMatches.SingleAsync(m => m.MatchId == match.MatchId));
        Assert.Equal(MatchStatus.Rejected, storedMatch.MatchStatus);
    }

    [Fact]
    public async Task Cancel_a_released_item_can_be_confirmed_for_someone_else()
    {
        var (report, item, _) = await S.ConfirmedMatchAsync(S.OmarId);
        await S.Reports().CancelAsync(report.ReportId, S.OmarId);

        var saraReport = await S.AddReportAsync(S.SaraId);
        var saraMatch = await S.AddMatchAsync(saraReport.ReportId, item.ItemId);

        await S.Matching().ConfirmAsync(saraMatch.MatchId, S.EmpSadatId);
    }

    [Fact]
    public async Task Cancel_is_forbidden_for_other_passengers_and_not_repeatable()
    {
        var report = await S.AddReportAsync(S.OmarId);

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Reports().CancelAsync(report.ReportId, S.SaraId));

        await S.Reports().CancelAsync(report.ReportId, S.OmarId);
        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Reports().CancelAsync(report.ReportId, S.OmarId));
        await Assert.ThrowsAsync<NotFoundException>(() => S.Reports().CancelAsync(999, S.OmarId));
    }
}
