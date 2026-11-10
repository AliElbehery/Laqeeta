using System.Security.Claims;
using Laqeeta.Api.Auth;
using Laqeeta.Api.Web.Models;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Laqeeta.Api.Web.Controllers;

[Route("account")]
public class AccountController(IAuthService auth) : AppController
{
    [HttpGet("login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome();
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var result = await auth.LoginAsync(new LoginDto(vm.UsernameOrEmail, vm.Password));
            await SignInAsync(result.User, vm.RememberMe);
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl) : RedirectToHome(result.User.Role);
        }
        catch (Exception ex) when (IsDomain(ex))
        {
            ModelState.AddModelError("", Msg(ex));
            return View(vm);
        }
    }

    [HttpGet("register"), AllowAnonymous]
    public IActionResult Register()
        => User.Identity?.IsAuthenticated == true ? RedirectToHome() : View(new RegisterVm());

    [HttpPost("register"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var result = await auth.RegisterAsync(
                new RegisterDto(vm.FullName, vm.Username, vm.Email, vm.Phone, vm.Password));
            await SignInAsync(result.User, persistent: false);
            Success(L["msg.welcome"]);
            return RedirectToHome(result.User.Role);
        }
        catch (Exception ex) when (IsDomain(ex))
        {
            ModelState.AddModelError("", Msg(ex));
            return View(vm);
        }
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(WebAuth.Scheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("profile"), Authorize]
    public async Task<IActionResult> Profile()
    {
        ViewData["Profile"] = await auth.GetProfileAsync(UserId);
        return View(new ChangePasswordVm());
    }

    [HttpPost("profile"), Authorize]
    public async Task<IActionResult> Profile(ChangePasswordVm vm)
    {
        ViewData["Profile"] = await auth.GetProfileAsync(UserId);
        if (!ModelState.IsValid) return View(vm);

        var error = await RunAsync(() => auth.ChangePasswordAsync(
            UserId, new ChangePasswordDto(vm.CurrentPassword, vm.NewPassword)));
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(vm);
        }
        Success(L["msg.passwordChanged"]);
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("denied"), AllowAnonymous]
    public IActionResult Denied() => View();

    private IActionResult RedirectToHome(UserRole? role = null)
    {
        var currentRole = role?.ToString();
        if (currentRole is null)
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Employee")) return RedirectToAction("Index", "Staff");
            return RedirectToAction("Index", "Reports");
        }

        return role == UserRole.Admin
            ? RedirectToAction("Index", "Admin")
            : role == UserRole.Employee
                ? RedirectToAction("Index", "Staff")
                : RedirectToAction("Index", "Reports");
    }

    private async Task SignInAsync(UserDto u, bool persistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimNames.UserId, u.UserId.ToString()),
            new(ClaimNames.Name, u.Username),
            new(ClaimNames.Role, u.Role.ToString()),
            new("full_name", u.FullName)
        };
        if (u.StationId is long sid) claims.Add(new Claim(ClaimNames.StationId, sid.ToString()));

        var identity = new ClaimsIdentity(claims, WebAuth.Scheme, ClaimNames.Name, ClaimNames.Role);
        await HttpContext.SignInAsync(WebAuth.Scheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = persistent });
    }
}
