using System.ComponentModel.DataAnnotations;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Api.Web.Models;

public class LoginVm
{
    [Required] public string UsernameOrEmail { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class RegisterVm
{
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = "";
    [Required, StringLength(50, MinimumLength = 3), RegularExpression(@"^[A-Za-z0-9._-]+$",
        ErrorMessage = "Username may contain letters, digits, '.', '_' and '-' only.")]
    public string Username { get; set; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid phone number.")]
    public string Phone { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match."), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ChangePasswordVm
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string NewPassword { get; set; } = "";
    [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match."), DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = "";
}

public class ReportFormVm
{
    [Range(1, long.MaxValue, ErrorMessage = "Select a line.")] public long LineId { get; set; }
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid phone number.")] public string ContactPhone { get; set; } = "";
    public ItemCategory Category { get; set; } = ItemCategory.Phone;
    [Required, StringLength(50)] public string Color { get; set; } = "";
    public DateOnly LostDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly? ApproximateTime { get; set; }
    [StringLength(150)] public string? Direction { get; set; }
    [Required, StringLength(2000)] public string Description { get; set; } = "";
}

public class ReportDetailsVm
{
    public LostReport Report { get; set; } = null!;
    public IReadOnlyList<PossibleMatch> Matches { get; set; } = [];
    public Dictionary<long, OwnershipClaim> ClaimsByMatch { get; set; } = new();
}

public class FoundItemFormVm
{
    [Range(1, long.MaxValue, ErrorMessage = "Select a station.")] public long FoundStationId { get; set; }
    public ItemCategory Category { get; set; } = ItemCategory.Phone;
    [Required, StringLength(50)] public string Color { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [StringLength(2000)] public string? PrivateIdentifyingDetails { get; set; }
    public DateTime FoundAt { get; set; } = DateTime.Now;
}

public class TransferVm
{
    [Range(1, long.MaxValue)] public long ToStationId { get; set; }
}

public class VerifyVm
{
    public bool IdChecked { get; set; }
    public bool SecretAnswerPassed { get; set; }
    [StringLength(2000)] public string? Notes { get; set; }
}

public class VerifyPageVm
{
    public OwnershipClaim Claim { get; set; } = null!;
    public PossibleMatch? Match { get; set; }
    public VerifyVm Form { get; set; } = new();
}

public class StaffDashboardVm
{
    public string StationName { get; set; } = "";
    public int ItemsInStorage { get; set; }
    public int SuggestedMatches { get; set; }
    public int PendingClaims { get; set; }
    public int ReadyForHandOver { get; set; }
}
