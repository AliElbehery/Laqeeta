using Laqeeta.Api.Web.Models;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Employee pages: found items held at my station.</summary>
[Route("staff/items")]
[Authorize(Policy = "EmployeeOnly")]
public class ItemsController(IFoundItemService items, IStationService stations) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(FoundItemStatus? status)
    {
        ViewData["Status"] = status;
        return View(await items.GetByStationAsync(StationId, status));
    }

    [HttpGet("new")]
    public async Task<IActionResult> Create()
    {
        await LoadStationsAsync();
        return View(new FoundItemFormVm { FoundStationId = StationId });
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(FoundItemFormVm vm)
    {
        await LoadStationsAsync();
        if (!ModelState.IsValid) return View(vm);

        // the form sends local time; the system stores UTC
        var foundAtUtc = DateTime.SpecifyKind(vm.FoundAt, DateTimeKind.Local).ToUniversalTime();
        var dto = new CreateFoundItemDto(vm.FoundStationId, vm.Category, vm.Color.Trim(), vm.Description.Trim(),
            string.IsNullOrWhiteSpace(vm.PrivateIdentifyingDetails) ? null : vm.PrivateIdentifyingDetails.Trim(),
            foundAtUtc);

        long itemId = 0;
        var error = await RunAsync(async () => itemId = (await items.LogAsync(UserId, dto)).ItemId);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(vm);
        }
        Success(L["msg.itemLogged"]);
        return RedirectToAction(nameof(Details), new { id = itemId });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        try
        {
            var item = await items.GetByIdAsync(id);
            if (item.CurrentStationId != StationId)
                throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);

            ViewData["Stations"] = (await stations.GetStationsAsync(acceptsItems: true))
                .Where(s => s.StationId != item.CurrentStationId).ToList();
            return View(item);
        }
        catch (Exception ex) when (IsDomain(ex))
        {
            Error(Msg(ex));
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("{id:long}/transfer")]
    public async Task<IActionResult> Transfer(long id, TransferVm vm)
    {
        var ok = await FlashAsync(() => items.TransferAsync(id, vm.ToStationId, UserId), L["msg.itemTransferred"]);
        return ok ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadStationsAsync() => ViewData["Stations"] = await stations.GetStationsAsync();
}