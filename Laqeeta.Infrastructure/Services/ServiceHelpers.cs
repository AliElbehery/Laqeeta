using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
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
                   ?? throw new NotFoundException($"User {userId} not found.");

        if (!user.IsActive || user.Role != UserRole.Employee || user.StationId is null)
            throw new ForbiddenException("This action requires an active employee assigned to a station.");

        return user;
    }

    /// <summary>Adds a notification to the context (caller saves, so it stays in the same transaction).</summary>
    public static void Notify(AppDbContext db, long userId, NotificationType type, string title, string message,
                              LostReport? report = null, PossibleMatch? match = null)
        => db.Notifications.Add(new Notification
        {
            UserId = userId, Type = type, Title = title, Message = message,
            Report = report, Match = match
        });
}
