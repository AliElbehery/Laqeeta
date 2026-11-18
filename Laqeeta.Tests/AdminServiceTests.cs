using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

public sealed class AdminServiceTests : IDisposable
{
    private readonly Scenario S = new();
    public void Dispose() => S.Dispose();

    [Fact]
    public async Task GetDashboard_returns_aggregated_counts_for_all_main_entities()
    {
        var report = await S.AddReportAsync();
        var item = await S.AddItemAsync();
        var match = await S.AddMatchAsync(report.ReportId, item.ItemId);
        var (_, _, confirmedMatch) = await S.ConfirmedMatchAsync(S.SaraId);
        await S.AddClaimAsync(confirmedMatch.MatchId, S.SaraId);

        var dashboard = await S.Admin().GetDashboardAsync();

        Assert.Equal(3, dashboard.TotalEmployees);
        Assert.Equal(3, dashboard.ActiveEmployees);
        Assert.Equal(2, dashboard.TotalPassengers);
        Assert.Equal(4, dashboard.TotalStations);
        Assert.Equal(2, dashboard.TotalLostReports);
        Assert.Equal(1, dashboard.OpenLostReports);
        Assert.Equal(2, dashboard.TotalFoundItems);
        Assert.Equal(1, dashboard.ItemsInStorage);
        Assert.Equal(1, dashboard.PendingClaims);
        Assert.Equal(1, dashboard.SuggestedMatches);
    }

    [Fact]
    public async Task GetEmployees_returns_only_employees_in_name_order()
    {
        var employees = await S.Admin().GetEmployeesAsync(null);

        Assert.Equal(3, employees.Count);
        Assert.Equal(employees.OrderBy(x => x.FullName).Select(x => x.UserId), employees.Select(x => x.UserId));
        Assert.DoesNotContain(employees, x => x.UserId == S.OmarId || x.UserId == S.SaraId);
    }

    [Theory]
    [InlineData("attaba", 1)]
    [InlineData("test.com", 3)]
    [InlineData("Maadi", 1)]
    [InlineData("not-found", 0)]
    public async Task GetEmployees_search_matches_employee_fields_and_station(string search, int expected)
    {
        var employees = await S.Admin().GetEmployeesAsync(search);
        Assert.Equal(expected, employees.Count);
    }

    [Fact]
    public async Task GetEmployees_empty_search_returns_all_employees()
    {
        Assert.Equal(3, (await S.Admin().GetEmployeesAsync("  ")).Count);
    }

    [Fact]
    public async Task ToggleEmployeeActive_changes_only_the_target_employee()
    {
        var result = await S.Admin().ToggleEmployeeActiveAsync(S.EmpSadatId, S.OmarId);

        Assert.Equal(ToggleEmployeeResult.Updated, result);
        Assert.False(await S.Read(db => db.Users.Where(x => x.UserId == S.EmpSadatId).Select(x => x.IsActive).SingleAsync()));
        Assert.True(await S.Read(db => db.Users.Where(x => x.UserId == S.EmpAttabaId).Select(x => x.IsActive).SingleAsync()));

        Assert.Equal(ToggleEmployeeResult.Updated,
            await S.Admin().ToggleEmployeeActiveAsync(S.EmpSadatId, S.OmarId));
        Assert.True(await S.Read(db => db.Users.Where(x => x.UserId == S.EmpSadatId).Select(x => x.IsActive).SingleAsync()));
    }

    [Fact]
    public async Task ToggleEmployeeActive_cannot_change_the_current_admin_account()
    {
        Assert.Equal(ToggleEmployeeResult.CannotChangeOwnAccount,
            await S.Admin().ToggleEmployeeActiveAsync(S.EmpSadatId, S.EmpSadatId));
        Assert.True(await S.Read(db => db.Users.Where(x => x.UserId == S.EmpSadatId).Select(x => x.IsActive).SingleAsync()));
    }

    [Fact]
    public async Task ToggleEmployeeActive_returns_not_found_for_missing_or_non_employee_users()
    {
        Assert.Equal(ToggleEmployeeResult.NotFound,
            await S.Admin().ToggleEmployeeActiveAsync(999, S.OmarId));
        Assert.Equal(ToggleEmployeeResult.NotFound,
            await S.Admin().ToggleEmployeeActiveAsync(S.OmarId, S.SaraId));
    }
}