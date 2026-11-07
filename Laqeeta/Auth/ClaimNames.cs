using System.Security.Claims;
using Laqeeta.Core.Exceptions;

namespace Laqeeta.Api.Auth;

public static class ClaimNames
{
    public const string UserId = "sub";
    public const string Name = "name";
    public const string Role = "role";
    public const string StationId = "station_id";
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>The id of the logged-in user (always taken from the token, never from the request body).</summary>
    public static long GetUserId(this ClaimsPrincipal user)
        => long.TryParse(user.FindFirst(ClaimNames.UserId)?.Value, out var id)
            ? id
            : throw new UnauthorizedException("Invalid token.");

    public static long? GetStationId(this ClaimsPrincipal user)
        => long.TryParse(user.FindFirst(ClaimNames.StationId)?.Value, out var id) ? id : null;
}
