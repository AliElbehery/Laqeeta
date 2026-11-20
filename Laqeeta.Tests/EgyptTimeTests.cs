using Laqeeta.Core.Time;

namespace Laqeeta.Tests;

/// <summary>
/// January dates are used for the plain cases on purpose: Cairo is UTC+2 there in every version of the time-zone database.
/// Egypt has had daylight saving time again since 2023 (UTC+3): the clocks go forward at 00:00 on the last Friday of April.
/// </summary>
public class EgyptTimeTests
{
    private static DateTimeOffset Utc(int y, int m, int d, int h, int min) => new(y, m, d, h, min, 0, TimeSpan.Zero);

    [Fact]
    public void A_cairo_day_starts_at_midnight_cairo_which_is_22_00_utc_the_day_before_in_winter()
        => Assert.Equal(Utc(2025, 1, 14, 22, 0), EgyptTime.StartOfDayUtc(new DateOnly(2025, 1, 15)));

    [Fact]
    public void DateOf_is_the_cairo_day_not_the_utc_day()
    {
        Assert.Equal(new DateOnly(2025, 1, 15), EgyptTime.DateOf(Utc(2025, 1, 14, 22, 30)));   // 00:30 Cairo
        Assert.Equal(new DateOnly(2025, 1, 14), EgyptTime.DateOf(Utc(2025, 1, 14, 21, 30)));   // 23:30 Cairo
    }

    [Fact]
    public void On_the_day_the_clocks_go_forward_midnight_does_not_exist_and_the_day_starts_at_01_00()
    {
        // Egypt 2025: 00:00 on Friday 25 April -> 01:00. ConvertTimeToUtc(00:00) used to throw here.
        // 01:00 at UTC+3 and (with an old database without daylight saving) 00:00 at UTC+2 are the same instant.
        Assert.Equal(Utc(2025, 4, 24, 22, 0), EgyptTime.StartOfDayUtc(new DateOnly(2025, 4, 25)));
    }

    [Fact]
    public void In_summer_cairo_is_three_hours_ahead_of_utc()
    {
        if (!EgyptTime.Zone.IsDaylightSavingTime(Utc(2025, 7, 1, 12, 0)))
            return;   // an old time-zone database without Egypt's daylight saving time: nothing to assert

        Assert.Equal(new DateOnly(2025, 7, 15), EgyptTime.DateOf(Utc(2025, 7, 14, 21, 30)));   // 00:30 Cairo
        Assert.Equal(Utc(2025, 7, 14, 21, 0), EgyptTime.StartOfDayUtc(new DateOnly(2025, 7, 15)));
    }
}

/// <summary>Matching a Cairo calendar day (LostDate) against a UTC instant (FoundAt), around midnight and the daylight-saving jump.</summary>
public class EgyptTimeMatchingTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    private static DateTime Utc(int y, int m, int d, int h, int min) => new(y, m, d, h, min, 0, DateTimeKind.Utc);

    // the clocks went forward at 00:00 on this day, so it starts at 22:00 UTC the day before
    private static readonly DateOnly SkippedMidnightDay = new(2025, 4, 25);

    [Fact]
    public async Task A_report_whose_lost_day_has_no_midnight_can_still_be_matched()
    {
        var report = await S.AddReportAsync(lostDate: SkippedMidnightDay);
        await S.AddItemAsync(foundAt: Utc(2025, 4, 24, 22, 30));       // 01:30 Cairo on the 25th

        // used to throw ArgumentException (invalid time) -> HTTP 500 right after the report had been saved
        Assert.Single(await S.Matching().GenerateForReportAsync(report.ReportId));
    }

    [Fact]
    public async Task On_that_day_an_item_found_before_the_day_started_does_not_match()
    {
        var report = await S.AddReportAsync(lostDate: SkippedMidnightDay);
        var item = await S.AddItemAsync(foundAt: Utc(2025, 4, 24, 21, 30));   // 23:30 Cairo on the 24th

        Assert.Empty(await S.Matching().GenerateForReportAsync(report.ReportId));
        Assert.Empty(await S.Matching().GenerateForItemAsync(item.ItemId));
    }

    [Fact]
    public async Task On_that_day_both_directions_agree_for_an_item_found_after_it_started()
    {
        await S.AddReportAsync(lostDate: SkippedMidnightDay);
        var item = await S.AddItemAsync(foundAt: Utc(2025, 4, 24, 22, 30));

        Assert.Single(await S.Matching().GenerateForItemAsync(item.ItemId));
    }

    [Fact]
    public async Task An_item_found_just_after_cairo_midnight_matches_that_days_report()
    {
        // 00:30 Cairo on 15 January is still 14 January in UTC
        await S.AddItemAsync(foundAt: Utc(2025, 1, 14, 22, 30));
        var report = await S.AddReportAsync(lostDate: new DateOnly(2025, 1, 15));

        Assert.Single(await S.Matching().GenerateForReportAsync(report.ReportId));
    }
}
