using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class PossibleMatch
{
    public long MatchId { get; set; }
    public long ReportId { get; set; }
    public long ItemId { get; set; }
    public MatchStatus MatchStatus { get; set; } = MatchStatus.Suggested;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LostReport Report { get; set; } = null!;
    public FoundItem Item { get; set; } = null!;
    public OwnershipClaim? Claim { get; set; }          // 1 : 0..1
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
