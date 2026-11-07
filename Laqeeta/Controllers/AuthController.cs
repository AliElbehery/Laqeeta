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

    /// <summary>An existing employee creates another employee account for a station.</summary>
    [HttpPost("employees")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<UserDto>> CreateEmployee(CreateEmployeeDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await auth.CreateEmployeeAsync(dto, ct));
}
