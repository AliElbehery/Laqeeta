using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize]
public class ClaimsController(IClaimService claims) : ControllerBase
{
    // ------------------------------------------------------------ passenger

    /// <summary>Claim ownership of a confirmed match.</summary>
    [HttpPost]
    [Authorize(Policy = "PassengerOnly")]
    public async Task<ActionResult<ClaimDto>> Submit(SubmitClaimDto dto, CancellationToken ct)
    {
        var claim = await claims.SubmitAsync(dto.MatchId, User.GetUserId(), ct);
        return StatusCode(StatusCodes.Status201Created, claim.ToDto());
    }

    [HttpGet("mine")]
    [Authorize(Policy = "PassengerOnly")]
    public async Task<ActionResult<IEnumerable<ClaimDto>>> GetMine(CancellationToken ct)
        => Ok((await claims.GetMineAsync(User.GetUserId(), ct)).Select(c => c.ToDto()));

    // ------------------------------------------------------------ employee

    /// <summary>Claims on items held at my station.</summary>
    [HttpGet]
    [Authorize(Policy = "EmployeeOnly")]
    public async Task<ActionResult<IEnumerable<ClaimDto>>> GetForMyStation(
        [FromQuery] ClaimStatus? status, CancellationToken ct)
        => Ok((await claims.GetForStationAsync(User.GetRequiredStationId(), status, ct)).Select(c => c.ToDto(staff: true)));

    /// <summary>Approve or reject a claim after checking ID and the secret answer.</summary>
    [HttpPost("{id:long}/verify")]
    [Authorize(Policy = "EmployeeOnly")]
    public async Task<ActionResult<ClaimDto>> Verify(long id, VerifyClaimDto dto, CancellationToken ct)
        => (await claims.VerifyAsync(id, User.GetUserId(), dto, ct)).ToDto(staff: true);

    [HttpPost("{id:long}/hand-over")]
    [Authorize(Policy = "EmployeeOnly")]
    public async Task<ActionResult<ClaimDto>> HandOver(long id, CancellationToken ct)
        => (await claims.HandOverAsync(id, User.GetUserId(), ct)).ToDto(staff: true);
}