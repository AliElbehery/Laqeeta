using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Laqeeta.Api.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Global administration area. Every action requires the Admin role.</summary>
[Route("admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IAdminService adminService) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dashboard = await adminService.GetDashboardAsync(ct);
        var vm = new AdminDashboardVm
        {
            TotalEmployees = dashboard.TotalEmployees,
            ActiveEmployees = dashboard.ActiveEmployees,
            TotalPassengers = dashboard.TotalPassengers,
            TotalStations = dashboard.TotalStations,
            TotalLostReports = dashboard.TotalLostReports,
            OpenLostReports = dashboard.OpenLostReports,
            TotalFoundItems = dashboard.TotalFoundItems,
            ItemsInStorage = dashboard.ItemsInStorage,
            PendingClaims = dashboard.PendingClaims,
            SuggestedMatches = dashboard.SuggestedMatches
        };

        return View(vm);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> Employees(string? search, CancellationToken ct)
    {
        var employees = await adminService.GetEmployeesAsync(search, ct);
        var vm = new AdminEmployeesVm
        {
            Search = search,
            Employees = employees.Select(x => new AdminEmployeeRowVm
            {
                UserId = x.UserId,
                FullName = x.FullName,
                Username = x.Username,
                Email = x.Email,
                Phone = x.Phone,
                StationName = x.StationName,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost("employees/{id:long}/toggle-active")]
    public async Task<IActionResult> ToggleEmployeeActive(long id, CancellationToken ct)
    {
        var result = await adminService.ToggleEmployeeActiveAsync(id, UserId, ct);

        switch (result)
        {
            case ToggleEmployeeResult.CannotChangeOwnAccount:
                Error("You cannot deactivate your own account.");
                break;
            case ToggleEmployeeResult.NotFound:
                Error("Employee not found.");
                break;
            case ToggleEmployeeResult.Updated:
                Success("Employee account status updated.");
                break;
        }

        return RedirectToAction(nameof(Employees));
    }
}