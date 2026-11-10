using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class OwnershipClaim : IHasConcurrencyStamp
{
    /// <summary>Optimistic-concurrency token (see <see cref="IHasConcurrencyStamp"/>).</summary>
    public Guid ConcurrencyStamp { get; set; }

    public long ClaimId { get; set; }
    public long MatchId { get; set; }                   // unique
    public long ClaimedBy { get; set; }
    public long? VerifiedBy { get; set; }
    public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Pending;
    public bool IdChecked { get; set; }
    public bool SecretAnswerPassed { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? HandedOverAt { get; set; }
    public string? VerificationNotes { get; set; }

    public PossibleMatch Match { get; set; } = null!;
    public User ClaimedByUser { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
