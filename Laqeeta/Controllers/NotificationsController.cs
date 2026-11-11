using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine(
        [FromQuery] bool unreadOnly, CancellationToken ct)
    {
        var lang = Messages.Normalize(Request.Headers.AcceptLanguage.ToString());   // "Accept-Language: ar" => Arabic
        return Ok((await notifications.GetForUserAsync(User.GetUserId(), unreadOnly, ct)).Select(n => n.ToDto(lang)));
    }

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id, CancellationToken ct)
    {
        await notifications.MarkAsReadAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await notifications.MarkAllAsReadAsync(User.GetUserId(), ct);
        return NoContent();
    }
}