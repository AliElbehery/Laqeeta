using Laqeeta.Api.Web.Models;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

[Route("staff")]
[Authorize(Policy = "EmployeeOnly")]
public class StaffController(
    IFoundItemService items, IMatchingService matching, IClaimService claims, IStationService stations) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var sid = StationId;
        var station = (await stations.GetStationsAsync()).FirstOrDefault(s => s.StationId == sid);

        return View(new StaffDashboardVm
        {
            StationName = station?.StationName ?? $"#{sid}",
            ItemsInStorage = (await items.GetByStationAsync(sid, FoundItemStatus.InStorage)).Count,
            SuggestedMatches = (await matching.GetForStationAsync(sid, MatchStatus.Suggested)).Count,
            PendingClaims = (await claims.GetForStationAsync(sid, ClaimStatus.Pending)).Count,
            ReadyForHandOver = (await claims.GetForStationAsync(sid, ClaimStatus.Verified)).Count
        });
    }
}