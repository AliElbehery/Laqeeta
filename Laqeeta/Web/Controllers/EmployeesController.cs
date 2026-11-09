using Laqeeta.Api.Web.Models;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Employee page: create another employee account for a station.</summary>
[Route("staff/employees")]
[Authorize(Policy = "EmployeeOnly")]
public class EmployeesController(IAuthService auth, IStationService stations) : AppController
{
    [HttpGet("new")]
    public async Task<IActionResult> Create()
    {
        await LoadStationsAsync();
        return View(new EmployeeFormVm { StationId = StationId });   // default: my own station
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(EmployeeFormVm vm)
    {
        await LoadStationsAsync();
        if (!ModelState.IsValid) return View(vm);

        var dto = new CreateEmployeeDto(vm.FullName.Trim(), vm.Username.Trim(), vm.Email.Trim(),
                                        vm.Phone.Trim(), vm.Password, vm.StationId);

        var error = await RunAsync(() => auth.CreateEmployeeAsync(dto));
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(vm);
        }

        Success(string.Format(L["msg.employeeCreated"], dto.Username));
        return RedirectToAction(nameof(Create));
    }

    private async Task LoadStationsAsync() => ViewData["Stations"] = await stations.GetStationsAsync();
}
