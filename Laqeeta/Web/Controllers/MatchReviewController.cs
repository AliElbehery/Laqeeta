using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Employee pages: review suggested matches for items held at my station.</summary>
[Route("staff/matches")]
[Authorize(Policy = "EmployeeOnly")]
public class MatchReviewController(IMatchingService matching) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(MatchStatus? status = MatchStatus.Suggested)
    {
        ViewData["Status"] = status;
        return View(await matching.GetForStationAsync(StationId, status));
    }

    [HttpPost("{id:long}/confirm")]
    public async Task<IActionResult> Confirm(long id, MatchStatus? status)
    {
        await FlashAsync(() => matching.ConfirmAsync(id, UserId), L["msg.matchConfirmed"]);
        return RedirectToAction(nameof(Index), new { status });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, MatchStatus? status)
    {
        await FlashAsync(() => matching.RejectAsync(id, UserId), L["msg.matchRejected"]);
        return RedirectToAction(nameof(Index), new { status });
    }
}