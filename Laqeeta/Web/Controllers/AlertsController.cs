using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

/// <summary>Notifications page (any logged-in user).</summary>
[Route("notifications")]
[Authorize]
public class AlertsController(INotificationService notifications) : AppController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(bool unreadOnly = false)
    {
        ViewData["UnreadOnly"] = unreadOnly;
        return View(await notifications.GetForUserAsync(UserId, unreadOnly));
    }

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkRead(long id, string? returnUrl = null)
    {
        await RunAsync(() => notifications.MarkAsReadAsync(id, UserId));
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await notifications.MarkAllAsReadAsync(UserId);
        Success(L["msg.allRead"]);
        return RedirectToAction(nameof(Index));
    }
}
