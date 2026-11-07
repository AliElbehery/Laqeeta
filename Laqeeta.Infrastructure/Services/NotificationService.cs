using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<Notification> CreateAsync(long userId, NotificationType type, string title, string message,
                                                long? reportId = null, long? matchId = null,
                                                CancellationToken ct = default)
    {
        var n = new Notification
        {
            UserId = userId, Type = type, Title = title, Message = message,
            ReportId = reportId, MatchId = matchId
        };
        db.Notifications.Add(n);
        await db.SaveChangesAsync(ct);
        return n;
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(long userId, bool unreadOnly = false,
                                                                   CancellationToken ct = default)
    {
        var q = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt).ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.NotificationId == notificationId, ct)
                ?? throw new NotFoundException("Notification not found.");
        if (n.UserId != userId) throw new ForbiddenException("Not your notification.");
        if (n.IsRead) return;

        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(long userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), ct);
    }
}
