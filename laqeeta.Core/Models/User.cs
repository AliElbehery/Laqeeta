using Laqeeta.Core.Enums;

namespace Laqeeta.Core.Models;

public class User
{
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public long? StationId { get; set; }          // employees only
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Station? Station { get; set; }
    public ICollection<LostReport> LostReports { get; set; } = new List<LostReport>();
    public ICollection<FoundItem> LoggedItems { get; set; } = new List<FoundItem>();
    public ICollection<OwnershipClaim> Claims { get; set; } = new List<OwnershipClaim>();
    public ICollection<OwnershipClaim> VerifiedClaims { get; set; } = new List<OwnershipClaim>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
