using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class Notification
{
    public long NotificationId { get; set; }
    public long UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long? ReportId { get; set; }
    public long? MatchId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
    public LostReport? Report { get; set; }
    public PossibleMatch? Match { get; set; }
}
