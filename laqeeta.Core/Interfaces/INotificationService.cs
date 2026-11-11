using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public interface INotificationService
{
    /// <summary>Creates and saves a notification.</summary>
    Task<Notification> CreateAsync(long userId, NotificationType type, string title, string message,
                                   long? reportId = null, long? matchId = null, CancellationToken ct = default);

    Task<IReadOnlyList<Notification>> GetForUserAsync(long userId, bool unreadOnly = false, CancellationToken ct = default);

    Task MarkAsReadAsync(long notificationId, long userId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(long userId, CancellationToken ct = default);
}