using Laqeeta.Api.Auth;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Components;

public class NotificationsBadgeViewComponent(INotificationService notifications) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;
        try
        {
            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
                count = (await notifications.GetForUserAsync(UserClaimsPrincipal.GetUserId(), unreadOnly: true)).Count;
        }
        catch { /* the badge must never break a page */ }
        return View(count);
    }
}
