using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

[ApiController]
[Route("api/stations")]
[AllowAnonymous]
public class StationsController(IStationService stations) : ControllerBase
{
    /// <summary>All metro lines (used to pick the line in a lost report).</summary>
    [HttpGet("lines")]
    public async Task<ActionResult<IReadOnlyList<LineDto>>> GetLines(CancellationToken ct)
        => Ok(await stations.GetLinesAsync(ct));

    /// <summary>Stations, optionally filtered by line and/or whether they accept lost items.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StationDto>>> GetStations(
        [FromQuery] long? lineId, [FromQuery] bool? acceptsItems, CancellationToken ct)
        => Ok(await stations.GetStationsAsync(lineId, acceptsItems, ct));
}
