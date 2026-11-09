using Laqeeta.Api.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Web.Controllers;

[AllowAnonymous]
public class HomeController : AppController
{
    [HttpGet("")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return User.IsInRole("Employee") ? RedirectToAction("Index", "Staff") : RedirectToAction("Index", "Reports");
        return View();
    }

    [HttpGet("lang/{lang}")]
    public IActionResult SetLanguage(string lang, string? returnUrl = null)
    {
        Response.Cookies.Append(Loc.CookieName, lang == "en" ? "en" : "ar", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax
        });
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpGet("error")]
    public IActionResult Error() => View();
}
