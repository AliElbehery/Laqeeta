using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

/// <summary>Employee side: found items held at the employee's own station.</summary>
[ApiController]
[Route("api/found-items")]
[Authorize(Policy = "EmployeeOnly")]
public class FoundItemsController(IFoundItemService items) : ControllerBase
{
    /// <summary>Log a found item. It is held at the employee's station.</summary>
    [HttpPost]
    public async Task<ActionResult<FoundItemDto>> Log(CreateFoundItemDto dto, CancellationToken ct)
    {
        var created = await items.LogAsync(User.GetUserId(), dto, ct);
        var full = await items.GetByIdAsync(created.ItemId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, full.ToStaffDto());
    }

    /// <summary>Items currently held at my station.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoundItemDto>>> GetMyStationItems(
        [FromQuery] FoundItemStatus? status, CancellationToken ct)
        => Ok((await items.GetByStationAsync(User.GetRequiredStationId(), status, ct)).Select(i => i.ToStaffDto()));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<FoundItemDto>> GetById(long id, CancellationToken ct)
    {
        var item = await items.GetByIdAsync(id, ct);
        if (item.CurrentStationId != User.GetRequiredStationId())
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);
        return item.ToStaffDto();
    }

    /// <summary>Move an item to another station that accepts items.</summary>
    [HttpPost("{id:long}/transfer")]
    public async Task<IActionResult> Transfer(long id, TransferItemDto dto, CancellationToken ct)
    {
        await items.TransferAsync(id, dto.ToStationId, User.GetUserId(), ct);
        return NoContent();
    }
}