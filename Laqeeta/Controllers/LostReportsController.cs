using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

/// <summary>Passenger side: lost reports and the matches found for them.</summary>
[ApiController]
[Route("api/lost-reports")]
[Authorize(Policy = "PassengerOnly")]
public class LostReportsController(ILostReportService reports, IMatchingService matching) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LostReportDto>> Create(CreateLostReportDto dto, CancellationToken ct)
    {
        var created = await reports.CreateAsync(User.GetUserId(), dto, ct);
        var full = await reports.GetByIdAsync(created.ReportId, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.ReportId }, full.ToDto());
    }

    /// <summary>My reports (newest first).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LostReportDto>>> GetMine(CancellationToken ct)
        => Ok((await reports.GetByUserAsync(User.GetUserId(), ct)).Select(r => r.ToDto()));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<LostReportDto>> GetById(long id, CancellationToken ct)
        => (await reports.GetByIdAsync(id, User.GetUserId(), ct)).ToDto();

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct)
    {
        await reports.CancelAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    /// <summary>Possible matches for my report (private identifying details are never included).</summary>
    [HttpGet("{id:long}/matches")]
    public async Task<ActionResult<IEnumerable<PassengerMatchDto>>> GetMatches(long id, CancellationToken ct)
        => Ok((await matching.GetForReportAsync(id, User.GetUserId(), ct)).Select(m => m.ToPassengerDto()));
}