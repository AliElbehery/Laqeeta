using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Laqeeta.Api.Auth;

public static class SessionStampValidator
{
    public static async Task<bool> IsValidAsync(
        ClaimsPrincipal? principal,
        AppDbContext db,
        CancellationToken ct = default)
    {
        var userIdValue = principal?.FindFirst(ClaimNames.UserId)?.Value;
        var role = principal?.FindFirst(ClaimNames.Role)?.Value;
        var stampValue = principal?.FindFirst(ClaimNames.SecurityStamp)?.Value;
        if (!long.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(role))
            return false;

        // Existing sessions have no stamp; accept them only until that account changes its password.
        if (stampValue is null)
            return await db.Users.AsNoTracking().AnyAsync(
                user => user.UserId == userId && user.IsActive && user.Role.ToString() == role
                    && user.SecurityStamp == Guid.Empty,
                ct);

        if (!Guid.TryParse(stampValue, out var securityStamp))
            return false;

        return await db.Users.AsNoTracking().AnyAsync(
            user => user.UserId == userId && user.IsActive && user.Role.ToString() == role
                && user.SecurityStamp == securityStamp,
            ct);
    }
}
