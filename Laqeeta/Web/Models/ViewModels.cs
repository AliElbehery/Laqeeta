using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Laqeeta.Api.Web.Models;

public class LoginVm
{
    [Required(ErrorMessage = "val.required")] public string UsernameOrEmail { get; set; } = "";
    [Required(ErrorMessage = "val.required"), DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class RegisterVm
{
    [Required(ErrorMessage = "val.required"), StringLength(150, MinimumLength = 2, ErrorMessage = "val.length")] public string FullName { get; set; } = "";

    [Required(ErrorMessage = "val.required"), StringLength(50, MinimumLength = 3, ErrorMessage = "val.length"), RegularExpression(@"^[A-Za-z0-9._-]+$",
        ErrorMessage = "val.username")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "val.required"), EmailAddress(ErrorMessage = "val.email"), StringLength(256, ErrorMessage = "val.length")] public string Email { get; set; } = "";

    [Required(ErrorMessage = "val.required"), RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "val.phone")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "val.required"), StringLength(100, MinimumLength = 8, ErrorMessage = "val.passwordLength"), DataType(DataType.Password)] public string Password { get; set; } = "";

    [Required(ErrorMessage = "val.required"), Compare(nameof(Password), ErrorMessage = "val.compare"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class ChangePasswordVm
{
    [Required(ErrorMessage = "val.required"), DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required(ErrorMessage = "val.required"), StringLength(100, MinimumLength = 8, ErrorMessage = "val.passwordLength"), DataType(DataType.Password)] public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "val.required"), Compare(nameof(NewPassword), ErrorMessage = "val.compare"), DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = "";
}

public class ReportFormVm
{
    [Range(1, long.MaxValue, ErrorMessage = "val.select")] public long LineId { get; set; }
    [Required(ErrorMessage = "val.required"), RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "val.phone")] public string ContactPhone { get; set; } = "";
    public ItemCategory Category { get; set; } = ItemCategory.Phone;
    [Required(ErrorMessage = "val.required"), StringLength(50, ErrorMessage = "val.length")] public string Color { get; set; } = "";
    public DateOnly LostDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly? ApproximateTime { get; set; }
    [StringLength(150, ErrorMessage = "val.length")] public string? Direction { get; set; }
    [Required(ErrorMessage = "val.required"), StringLength(2000, ErrorMessage = "val.length")] public string Description { get; set; } = "";
}

public class ReportDetailsVm
{
    public LostReport Report { get; set; } = null!;
    public IReadOnlyList<PossibleMatch> Matches { get; set; } = [];
    public Dictionary<long, OwnershipClaim> ClaimsByMatch { get; set; } = new();
}

public class FoundItemFormVm
{
    [Range(1, long.MaxValue, ErrorMessage = "val.select")] public long FoundStationId { get; set; }
    public ItemCategory Category { get; set; } = ItemCategory.Phone;
    [Required(ErrorMessage = "val.required"), StringLength(50, ErrorMessage = "val.length")] public string Color { get; set; } = "";
    [Required(ErrorMessage = "val.required"), StringLength(2000, ErrorMessage = "val.length")] public string Description { get; set; } = "";
    [StringLength(2000, ErrorMessage = "val.length")] public string? PrivateIdentifyingDetails { get; set; }
    public DateTime FoundAt { get; set; }
}

public class TransferVm
{
    [Range(1, long.MaxValue)] public long ToStationId { get; set; }
}

public class VerifyVm
{
    public bool IdChecked { get; set; }
    public bool SecretAnswerPassed { get; set; }
    [StringLength(2000, ErrorMessage = "val.length")] public string? Notes { get; set; }
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

public class EmployeeFormVm
{
    [Required(ErrorMessage = "val.required"), StringLength(150, MinimumLength = 2, ErrorMessage = "val.length")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "val.required"), StringLength(50, MinimumLength = 3, ErrorMessage = "val.length"),
     RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "val.username")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "val.required"), EmailAddress(ErrorMessage = "val.email"), StringLength(256, ErrorMessage = "val.length")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "val.required"), RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "val.phone")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "val.required"), StringLength(100, MinimumLength = 8, ErrorMessage = "val.passwordLength"), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "val.required"), Compare(nameof(Password), ErrorMessage = "val.compare"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    [Range(1, long.MaxValue, ErrorMessage = "val.select")]
    public long StationId { get; set; }
}

public class AdminDashboardVm
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalPassengers { get; set; }
    public int TotalStations { get; set; }
    public int TotalLostReports { get; set; }
    public int OpenLostReports { get; set; }
    public int TotalFoundItems { get; set; }
    public int ItemsInStorage { get; set; }
    public int PendingClaims { get; set; }
    public int SuggestedMatches { get; set; }
}

public class AdminEmployeeRowVm
{
    public long UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? StationName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminEmployeesVm
{
    public string? Search { get; set; }
    public IReadOnlyList<AdminEmployeeRowVm> Employees { get; set; } = [];
}