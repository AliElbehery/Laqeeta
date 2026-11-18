using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Tests;

public class NotificationServiceTests : IDisposable
{
    private readonly Scenario S = new();

    public void Dispose() => S.Dispose();

    [Fact]
    public async Task GetForUser_returns_only_my_notifications_and_can_filter_unread()
    {
        var read = await S.Notifications().CreateAsync(S.OmarId, NotificationType.System, "a", "a");
        await S.Notifications().CreateAsync(S.OmarId, NotificationType.System, "b", "b");
        await S.Notifications().CreateAsync(S.SaraId, NotificationType.System, "c", "c");
        await S.Notifications().MarkAsReadAsync(read.NotificationId, S.OmarId);

        Assert.Equal(2, (await S.Notifications().GetForUserAsync(S.OmarId)).Count);
        var unread = await S.Notifications().GetForUserAsync(S.OmarId, unreadOnly: true);
        Assert.Equal("b", Assert.Single(unread).Title);
    }

    [Fact]
    public async Task MarkAsRead_sets_the_read_time_and_is_forbidden_for_other_users()
    {
        var n = await S.Notifications().CreateAsync(S.OmarId, NotificationType.System, "t", "m");

        await Assert.ThrowsAsync<ForbiddenException>(() => S.Notifications().MarkAsReadAsync(n.NotificationId, S.SaraId));
        await Assert.ThrowsAsync<NotFoundException>(() => S.Notifications().MarkAsReadAsync(999, S.OmarId));

        await S.Notifications().MarkAsReadAsync(n.NotificationId, S.OmarId);
        var stored = await S.Read(db => db.Notifications.SingleAsync(x => x.NotificationId == n.NotificationId));
        Assert.True(stored.IsRead);
        Assert.NotNull(stored.ReadAt);
    }

    [Fact]
    public async Task MarkAllAsRead_only_touches_my_notifications()
    {
        await S.Notifications().CreateAsync(S.OmarId, NotificationType.System, "a", "a");
        await S.Notifications().CreateAsync(S.OmarId, NotificationType.System, "b", "b");
        await S.Notifications().CreateAsync(S.SaraId, NotificationType.System, "c", "c");

        await S.Notifications().MarkAllAsReadAsync(S.OmarId);

        Assert.Empty(await S.Notifications().GetForUserAsync(S.OmarId, unreadOnly: true));
        Assert.Single(await S.Notifications().GetForUserAsync(S.SaraId, unreadOnly: true));
    }
}