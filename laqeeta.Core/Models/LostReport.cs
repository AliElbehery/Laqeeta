using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class LostReport : IHasConcurrencyStamp
{
    /// <summary>Optimistic-concurrency token (see <see cref="IHasConcurrencyStamp"/>).</summary>
    public Guid ConcurrencyStamp { get; set; }

    public long ReportId { get; set; }
    public long UserId { get; set; }
    public long LineId { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public string Color { get; set; } = string.Empty;
    public DateOnly LostDate { get; set; }
    public TimeOnly? ApproximateTime { get; set; }
    public string? Direction { get; set; }
    public string Description { get; set; } = string.Empty;
    public LostReportStatus Status { get; set; } = LostReportStatus.Open;
    public ClosureReason? ClosureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public MetroLine Line { get; set; } = null!;
    public ICollection<PossibleMatch> Matches { get; set; } = new List<PossibleMatch>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}