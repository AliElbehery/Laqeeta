using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<IReadOnlyList<Notification>> GetForUserAsync(long userId, bool unreadOnly, CancellationToken ct);
    Task<Notification?> GetByIdAsync(long notificationId, CancellationToken ct);
    Task MarkAllAsReadAsync(long userId, DateTime now, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}