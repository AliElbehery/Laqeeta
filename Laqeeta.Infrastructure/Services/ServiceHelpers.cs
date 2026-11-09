using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

internal static class ServiceHelpers
{
    /// <summary>Loads an active employee that is assigned to a station, or throws.</summary>
    public static async Task<User> RequireEmployeeAsync(AppDbContext db, long userId, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct)
                   ?? throw new NotFoundException($"User {userId} not found.", Err.UserNotFound);

        if (!user.IsActive || user.Role != UserRole.Employee || user.StationId is null)
            throw new ForbiddenException("This action requires an active employee assigned to a station.", Err.EmployeeRequired);

        return user;
    }

    /// <summary>Adds a notification to the context (caller saves, so it stays in the same transaction).</summary>
    /// <remarks>
    /// <paramref name="title"/> / <paramref name="message"/> stay in English (fallback for old rows and API clients);
    /// <paramref name="code"/> + <paramref name="param"/> let the website and API show them in the user's language.
    /// </remarks>
    public static void Notify(AppDbContext db, long userId, NotificationType type, string title, string message,
                              LostReport? report = null, PossibleMatch? match = null,
                              string? code = null, string? param = null)
        => db.Notifications.Add(new Notification
        {
            UserId = userId, Type = type, Title = title, Message = message,
            Report = report, Match = match, Code = code, Param = param
        });
}
