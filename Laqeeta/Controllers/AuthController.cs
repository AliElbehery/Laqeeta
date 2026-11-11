using Laqeeta.Api.Auth;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Laqeeta.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Passenger self-registration.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await auth.RegisterAsync(dto, ct));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto dto, CancellationToken ct)
        => await auth.LoginAsync(dto, ct);

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
        => await auth.GetProfileAsync(User.GetUserId(), ct);

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(User.GetUserId(), dto, ct);
        return NoContent();
    }

    /// <summary>Only an administrator can create employee accounts.</summary>
    [HttpPost("employees")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> CreateEmployee(CreateEmployeeDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await auth.CreateEmployeeAsync(dto, ct));
}