using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

/// <summary>Employee side: review the suggested matches for items held at my station.</summary>
[ApiController]
[Route("api/matches")]
[Authorize(Policy = "EmployeeOnly")]
public class MatchesController(IMatchingService matching) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StaffMatchDto>>> GetForMyStation(
        [FromQuery] MatchStatus? status, CancellationToken ct)
        => Ok((await matching.GetForStationAsync(User.GetRequiredStationId(), status, ct)).Select(m => m.ToStaffDto()));

    [HttpPost("{id:long}/confirm")]
    public async Task<IActionResult> Confirm(long id, CancellationToken ct)
    {
        await matching.ConfirmAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, CancellationToken ct)
    {
        await matching.RejectAsync(id, User.GetUserId(), ct);
        return NoContent();
    }
}
