using Laqeeta.Core.Enums;

namespace Laqeeta.Core.DTOs;

public record LostReportDto(
    long ReportId, long UserId, long LineId, string? LineName, string ContactPhone, ItemCategory Category,
    string Color, DateOnly LostDate, TimeOnly? ApproximateTime, string? Direction, string Description,
    LostReportStatus Status, ClosureReason? ClosureReason, DateTime CreatedAt);

public record LostReportPageDto(IReadOnlyList<LostReportDto> Items, string? NextCursor);

/// <summary>What a passenger may see about a found item (no private identifying details).</summary>
public record ItemPublicDto(
    long ItemId, ItemCategory Category, string Color, string Description, DateTimeOffset FoundAt,
    long CurrentStationId, string? CurrentStationName);

/// <summary>Employee view of a found item (includes the private identifying details).</summary>
public record FoundItemDto(
    long ItemId, long FoundStationId, string? FoundStationName, long CurrentStationId, string? CurrentStationName,
    long LoggedBy, ItemCategory Category, string Color, string Description, string? PrivateIdentifyingDetails,
    DateTimeOffset FoundAt, DateTime LoggedAt, FoundItemStatus ItemStatus, DateTime? ReturnedAt);

public record PassengerMatchDto(long MatchId, long ReportId, MatchStatus MatchStatus, DateTime CreatedAt,
                                ItemPublicDto Item);

public record StaffMatchDto(long MatchId, MatchStatus MatchStatus, DateTime CreatedAt,
                            LostReportDto Report, FoundItemDto Item);

public record ClaimDto(
    long ClaimId, long MatchId, long? ReportId, long? ItemId, ClaimStatus ClaimStatus,
    DateTime ClaimedAt, DateTime? VerifiedAt, DateTime? HandedOverAt,
    // staff-only fields (null for passengers)
    long? ClaimedBy, string? ClaimedByName, string? ClaimedByPhone, bool? IdChecked, bool? SecretAnswerPassed,
    string? VerificationNotes);

public record NotificationDto(
    long NotificationId, NotificationType Type, string Title, string Message,
    long? ReportId, long? MatchId, bool IsRead, DateTime CreatedAt, DateTime? ReadAt);