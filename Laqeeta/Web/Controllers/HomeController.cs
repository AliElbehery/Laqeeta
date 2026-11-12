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
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Employee")) return RedirectToAction("Index", "Staff");
            return RedirectToAction("Index", "Reports");
        return View();
    }

    [HttpGet("lang/{lang}")]
    public IActionResult SetLanguage(string lang, string? returnUrl = null)
    {
        Response.Cookies.Append(Loc.CookieName, lang == "en" ? "en" : "ar", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpGet("error")]
    public IActionResult Error() => View();
}