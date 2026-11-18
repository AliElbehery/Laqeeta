using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public void Add(Notification notification) => db.Notifications.Add(notification);
    public async Task<IReadOnlyList<Notification>> GetForUserAsync(long userId, bool unreadOnly, CancellationToken ct)
    {
        var q = db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt).ToListAsync(ct);
    }
    public Task<Notification?> GetByIdAsync(long id, CancellationToken ct)
        => db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id, ct);
    public Task MarkAllAsReadAsync(long userId, DateTime now, CancellationToken ct)
        => db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now), ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}