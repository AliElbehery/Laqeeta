using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class FoundItem : IHasConcurrencyStamp
{
    /// <summary>Optimistic-concurrency token (see <see cref="IHasConcurrencyStamp"/>).</summary>
    public Guid ConcurrencyStamp { get; set; }

    public long ItemId { get; set; }
    public long FoundStationId { get; set; }
    public long CurrentStationId { get; set; }
    public long LoggedBy { get; set; }
    public ItemCategory Category { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Hidden from the public; used by employees to verify ownership.</summary>
    public string? PrivateIdentifyingDetails { get; set; }

    public DateTime FoundAt { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public FoundItemStatus ItemStatus { get; set; } = FoundItemStatus.InStorage;
    public DateTime? ReturnedAt { get; set; }

    public Station FoundStation { get; set; } = null!;
    public Station CurrentStation { get; set; } = null!;
    public User LoggedByUser { get; set; } = null!;
    public ICollection<PossibleMatch> Matches { get; set; } = new List<PossibleMatch>();
}