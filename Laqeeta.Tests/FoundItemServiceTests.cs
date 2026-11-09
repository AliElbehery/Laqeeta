using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

public class FoundItemServiceTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    private CreateFoundItemDto Dto(long? foundStationId = null, DateTime? foundAt = null, string color = "Black")
        => new(foundStationId ?? S.SadatId, ItemCategory.Wallet, color, "Black leather wallet",
               "Blue card inside", foundAt ?? DateTime.UtcNow.AddMinutes(-30));

    // ------------------------------------------------------------ Log

    [Fact]
    public async Task Log_stores_the_item_at_the_employees_station()
    {
        var item = await S.Items().LogAsync(S.EmpAttabaId, Dto(foundStationId: S.SadatId, color: " Black "));

        var stored = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal(S.SadatId, stored.FoundStationId);     // where it was found
        Assert.Equal(S.AttabaId, stored.CurrentStationId);  // where the employee keeps it
        Assert.Equal(S.EmpAttabaId, stored.LoggedBy);
        Assert.Equal("Black", stored.Color);
        Assert.Equal(FoundItemStatus.InStorage, stored.ItemStatus);
    }

    [Fact]
    public async Task Log_immediately_looks_for_matching_reports()
    {
        var report = await S.AddReportAsync();

        var item = await S.Items().LogAsync(S.EmpSadatId, Dto());

        var match = await S.Read(db => db.PossibleMatches.SingleAsync(m => m.ItemId == item.ItemId));
        Assert.Equal(report.ReportId, match.ReportId);
        Assert.Equal(1, await S.Read(db => db.Notifications.CountAsync(n => n.UserId == S.OmarId)));
    }

    [Fact]
    public async Task Log_requires_a_station_that_accepts_items()
        => await Assert.ThrowsAsync<BusinessRuleException>(() => S.Items().LogAsync(S.EmpMaadiId, Dto()));

    [Fact]
    public async Task Log_rejects_an_unknown_found_station()
        => await Assert.ThrowsAsync<NotFoundException>(() => S.Items().LogAsync(S.EmpSadatId, Dto(foundStationId: 999)));

    [Fact]
    public async Task Log_rejects_a_found_time_in_the_future()
        => await Assert.ThrowsAsync<BusinessRuleException>(() =>
            S.Items().LogAsync(S.EmpSadatId, Dto(foundAt: DateTime.UtcNow.AddHours(2))));

    [Fact]
    public async Task Log_is_forbidden_for_passengers()
        => await Assert.ThrowsAsync<ForbiddenException>(() => S.Items().LogAsync(S.OmarId, Dto()));

    // ------------------------------------------------------------ Read

    [Fact]
    public async Task GetByStation_filters_by_station_and_status()
    {
        await S.AddItemAsync(heldAtId: S.SadatId);
        await S.AddItemAsync(heldAtId: S.SadatId, status: FoundItemStatus.Returned);
        await S.AddItemAsync(heldAtId: S.AttabaId);

        Assert.Equal(2, (await S.Items().GetByStationAsync(S.SadatId)).Count);
        Assert.Single(await S.Items().GetByStationAsync(S.SadatId, FoundItemStatus.Returned));
        Assert.Single(await S.Items().GetByStationAsync(S.AttabaId));
    }

    [Fact]
    public async Task GetById_includes_stations_and_throws_for_unknown_items()
    {
        var item = await S.AddItemAsync(heldAtId: S.AttabaId, foundStationId: S.SadatId);

        var found = await S.Items().GetByIdAsync(item.ItemId);

        Assert.Equal("Sadat", found.FoundStation.StationName);
        Assert.Equal("Attaba", found.CurrentStation.StationName);
        await Assert.ThrowsAsync<NotFoundException>(() => S.Items().GetByIdAsync(999));
    }

    // ------------------------------------------------------------ Transfer

    [Fact]
    public async Task Transfer_moves_the_item_to_the_target_station()
    {
        var item = await S.AddItemAsync();

        await S.Items().TransferAsync(item.ItemId, S.AttabaId, S.EmpSadatId);

        var stored = await S.Read(db => db.FoundItems.SingleAsync(i => i.ItemId == item.ItemId));
        Assert.Equal(S.AttabaId, stored.CurrentStationId);
        Assert.Equal(S.SadatId, stored.FoundStationId);   // where it was found never changes
    }

    [Fact]
    public async Task Transfer_rejects_bad_targets()
    {
        var item = await S.AddItemAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Items().TransferAsync(item.ItemId, S.MaadiId, S.EmpSadatId)); // does not accept items
        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Items().TransferAsync(item.ItemId, S.SadatId, S.EmpSadatId)); // already there
        await Assert.ThrowsAsync<NotFoundException>(() => S.Items().TransferAsync(item.ItemId, 999, S.EmpSadatId));
    }

    [Fact]
    public async Task Transfer_is_forbidden_for_an_employee_of_another_station()
    {
        var item = await S.AddItemAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Items().TransferAsync(item.ItemId, S.ShubraId, S.EmpAttabaId));
    }

    [Fact]
    public async Task Transfer_only_works_for_items_in_storage()
    {
        var item = await S.AddItemAsync(status: FoundItemStatus.Matched);

        await Assert.ThrowsAsync<BusinessRuleException>(() => S.Items().TransferAsync(item.ItemId, S.AttabaId, S.EmpSadatId));
    }
}
