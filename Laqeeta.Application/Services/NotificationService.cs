using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Services;

public sealed class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task<Notification> CreateAsync(long userId, NotificationType type, string title, string message,
        long? reportId = null, long? matchId = null, CancellationToken ct = default)
    {
        var notification = new Notification { UserId = userId, Type = type, Title = title, Message = message,
            ReportId = reportId, MatchId = matchId };
        repository.Add(notification);
        await repository.SaveChangesAsync(ct);
        return notification;
    }

    public Task<IReadOnlyList<Notification>> GetForUserAsync(long userId, bool unreadOnly = false, CancellationToken ct = default)
        => repository.GetForUserAsync(userId, unreadOnly, ct);

    public async Task MarkAsReadAsync(long notificationId, long userId, CancellationToken ct = default)
    {
        var notification = await repository.GetByIdAsync(notificationId, ct)
            ?? throw new NotFoundException("Notification not found.", Err.NotificationNotFound);
        if (notification.UserId != userId) throw new ForbiddenException("Not your notification.", Err.NotYourNotification);
        if (notification.IsRead) return;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(ct);
    }

    public Task MarkAllAsReadAsync(long userId, CancellationToken ct = default)
        => repository.MarkAllAsReadAsync(userId, DateTime.UtcNow, ct);
}