using Laqeeta.Api.Web.Models;
using Laqeeta.Core.Enums;
using Laqeeta.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Global administration area. Every action requires the Admin role.</summary>
[Route("admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(AppDbContext db) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new AdminDashboardVm
        {
            TotalEmployees = await db.Users.CountAsync(x => x.Role == UserRole.Employee, ct),
            ActiveEmployees = await db.Users.CountAsync(x => x.Role == UserRole.Employee && x.IsActive, ct),
            TotalPassengers = await db.Users.CountAsync(x => x.Role == UserRole.User, ct),
            TotalStations = await db.Stations.CountAsync(ct),
            TotalLostReports = await db.LostReports.CountAsync(ct),
            OpenLostReports = await db.LostReports.CountAsync(x => x.Status == LostReportStatus.Open, ct),
            TotalFoundItems = await db.FoundItems.CountAsync(ct),
            ItemsInStorage = await db.FoundItems.CountAsync(x => x.Status == FoundItemStatus.InStorage, ct),
            PendingClaims = await db.OwnershipClaims.CountAsync(x => x.Status == ClaimStatus.Pending, ct),
            SuggestedMatches = await db.PossibleMatches.CountAsync(x => x.Status == MatchStatus.Suggested, ct)
        };

        return View(vm);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> Employees(string? search, CancellationToken ct)
    {
        var query = db.Users.AsNoTracking()
            .Where(x => x.Role == UserRole.Employee);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FullName.Contains(term)
                || x.Username.Contains(term) || x.Email.Contains(term)
                || (x.Station != null && x.Station.StationName.Contains(term)));
        }

        var employees = await query
            .OrderBy(x => x.FullName)
            .Select(x => new AdminEmployeeRowVm
            {
                UserId = x.UserId,
                FullName = x.FullName,
                Username = x.Username,
                Email = x.Email,
                Phone = x.Phone,
                StationName = x.Station == null ? null : x.Station.StationName,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return View(new AdminEmployeesVm { Search = search, Employees = employees });
    }

    [HttpPost("employees/{id:long}/toggle-active")]
    public async Task<IActionResult> ToggleEmployeeActive(long id, CancellationToken ct)
    {
        if (id == UserId)
        {
            Error("You cannot deactivate your own account.");
            return RedirectToAction(nameof(Employees));
        }

        var employee = await db.Users.FirstOrDefaultAsync(
            x => x.UserId == id && x.Role == UserRole.Employee, ct);

        if (employee is null)
        {
            Error("Employee not found.");
            return RedirectToAction(nameof(Employees));
        }

        employee.IsActive = !employee.IsActive;
        await db.SaveChangesAsync(ct);

        Success(employee.IsActive ? "Employee account activated." : "Employee account deactivated.");
        return RedirectToAction(nameof(Employees));
    }
}
