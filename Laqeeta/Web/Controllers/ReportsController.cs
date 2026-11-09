using Laqeeta.Api.Web.Models;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Passenger pages: my lost reports, their matches and ownership claims.</summary>
[Route("reports")]
[Authorize(Policy = "PassengerOnly")]
public class ReportsController(
    ILostReportService reports, IMatchingService matching, IClaimService claims,
    IStationService stations, IAuthService auth) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await reports.GetByUserAsync(UserId));

    [HttpGet("new")]
    public async Task<IActionResult> Create()
    {
        var profile = await auth.GetProfileAsync(UserId);
        await LoadLinesAsync();
        return View(new ReportFormVm { ContactPhone = profile.Phone });
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(ReportFormVm vm)
    {
        await LoadLinesAsync();
        if (!ModelState.IsValid) return View(vm);

        var dto = new CreateLostReportDto(vm.LineId, vm.ContactPhone.Trim(), vm.Category, vm.Color.Trim(),
            vm.LostDate, vm.ApproximateTime,
            string.IsNullOrWhiteSpace(vm.Direction) ? null : vm.Direction.Trim(), vm.Description.Trim());

        long reportId = 0;
        var error = await RunAsync(async () => reportId = (await reports.CreateAsync(UserId, dto)).ReportId);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(vm);
        }
        Success(L["msg.reportCreated"]);
        return RedirectToAction(nameof(Details), new { id = reportId });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        try
        {
            var vm = new ReportDetailsVm
            {
                Report = await reports.GetByIdAsync(id, UserId),
                Matches = await matching.GetForReportAsync(id, UserId)
            };
            vm.ClaimsByMatch = (await claims.GetMineAsync(UserId)).ToDictionary(c => c.MatchId);
            return View(vm);
        }
        catch (Exception ex) when (IsDomain(ex))
        {
            Error(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await FlashAsync(() => reports.CancelAsync(id, UserId), L["msg.reportCancelled"]);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:long}/claim")]
    public async Task<IActionResult> Claim(long id, long matchId)
    {
        await FlashAsync(() => claims.SubmitAsync(matchId, UserId), L["msg.claimSubmitted"]);
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLinesAsync() => ViewData["Lines"] = await stations.GetLinesAsync();
}
