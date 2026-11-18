using Laqeeta.Core.Enums;

namespace Laqeeta.Tests;

public sealed class StationServiceTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    [Fact]
    public async Task GetLines_returns_all_lines_in_name_order()
    {
        var lines = await S.Stations().GetLinesAsync();
        Assert.Equal(2, lines.Count);
        Assert.Equal(new[] { "Line 1", "Line 2" }, lines.Select(x => x.LineName));
    }

    [Fact]
    public async Task GetStations_without_filters_returns_every_station_and_line_membership()
    {
        var stations = await S.Stations().GetStationsAsync();
        Assert.Equal(4, stations.Count);
        var attaba = Assert.Single(stations, x => x.StationId == S.AttabaId);
        Assert.Equal(new[] { "Line 1", "Line 2" }, attaba.Lines.OrderBy(x => x));
    }

    [Fact]
    public async Task GetStations_filters_by_line()
    {
        var line1 = await S.Stations().GetStationsAsync(lineId: S.Line1Id);
        Assert.Equal(3, line1.Count);
        Assert.DoesNotContain(line1, x => x.StationId == S.ShubraId);

        var line2 = await S.Stations().GetStationsAsync(lineId: S.Line2Id);
        Assert.Equal(2, line2.Count);
        Assert.Contains(line2, x => x.StationId == S.AttabaId);
    }

    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 1)]
    public async Task GetStations_filters_by_item_acceptance(bool acceptsItems, int expected)
    {
        var stations = await S.Stations().GetStationsAsync(acceptsItems: acceptsItems);
        Assert.Equal(expected, stations.Count);
        Assert.All(stations, station => Assert.Equal(acceptsItems, station.AcceptsItems));
    }

    [Fact]
    public async Task GetStations_combines_line_and_acceptance_filters()
    {
        var stations = await S.Stations().GetStationsAsync(S.Line1Id, acceptsItems: true);
        Assert.Equal(2, stations.Count);
        Assert.DoesNotContain(stations, x => x.StationId == S.MaadiId);
    }

    [Fact]
    public async Task GetStations_returns_empty_for_unknown_line()
    {
        Assert.Empty(await S.Stations().GetStationsAsync(lineId: 999));
    }
}