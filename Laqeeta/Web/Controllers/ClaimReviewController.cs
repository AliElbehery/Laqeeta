using Laqeeta.Api.Web.Models;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Employee pages: verify ownership claims and hand items over.</summary>
[Route("staff/claims")]
[Authorize(Policy = "EmployeeOnly")]
public class ClaimReviewController(IClaimService claims, IMatchingService matching) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(ClaimStatus? status)
    {
        ViewData["Status"] = status;
        return View(await claims.GetForStationAsync(StationId, status));
    }

    [HttpGet("{id:long}/verify")]
    public async Task<IActionResult> Verify(long id)
    {
        var page = await LoadPageAsync(id);
        return page is null ? RedirectToAction(nameof(Index)) : View(page);
    }

    [HttpPost("{id:long}/verify")]
    public async Task<IActionResult> Verify(long id, VerifyVm form, bool approve)
    {
        var dto = new VerifyClaimDto(approve, form.IdChecked, form.SecretAnswerPassed,
            string.IsNullOrWhiteSpace(form.Notes) ? null : form.Notes.Trim());

        var error = await RunAsync(() => claims.VerifyAsync(id, UserId, dto));
        if (error is null)
        {
            Success(approve ? L["msg.claimApproved"] : L["msg.claimRejected"]);
            return RedirectToAction(nameof(Index));
        }

        Error(error);
        var page = await LoadPageAsync(id);
        if (page is null) return RedirectToAction(nameof(Index));
        page.Form = form;
        return View(page);
    }

    [HttpPost("{id:long}/hand-over")]
    public async Task<IActionResult> HandOver(long id)
    {
        await FlashAsync(() => claims.HandOverAsync(id, UserId), L["msg.handedOver"]);
        return RedirectToAction(nameof(Index));
    }

    private async Task<VerifyPageVm?> LoadPageAsync(long id)
    {
        var sid = StationId;
        var claim = (await claims.GetForStationAsync(sid)).FirstOrDefault(c => c.ClaimId == id);
        if (claim is null)
        {
            Error(L["msg.claimNotFound"]);
            return null;
        }
        var match = (await matching.GetForStationAsync(sid)).FirstOrDefault(m => m.MatchId == claim.MatchId);
        return new VerifyPageVm { Claim = claim, Match = match };
    }
}
