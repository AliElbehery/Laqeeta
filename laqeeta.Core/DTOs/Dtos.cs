using Laqeeta.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Laqeeta.Core.DTOs;

public record CreateLostReportDto(
    [Range(1, long.MaxValue)] long LineId,
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid phone number.")] string ContactPhone,
    [EnumDataType(typeof(ItemCategory))] ItemCategory Category,
    [Required, StringLength(50)] string Color,
    DateOnly LostDate,
    TimeOnly? ApproximateTime,
    [StringLength(150)] string? Direction,
    [Required, StringLength(2000)] string Description);

public record CreateFoundItemDto(
    [Range(1, long.MaxValue)] long FoundStationId,
    [EnumDataType(typeof(ItemCategory))] ItemCategory Category,
    [Required, StringLength(50)] string Color,
    [Required, StringLength(2000)] string Description,
    [StringLength(2000)] string? PrivateIdentifyingDetails,
    DateTimeOffset FoundAt);

public record VerifyClaimDto(bool Approve, bool IdChecked, bool SecretAnswerPassed, [StringLength(2000)] string? Notes);

public record SubmitClaimDto([Range(1, long.MaxValue)] long MatchId);

public record TransferItemDto([Range(1, long.MaxValue)] long ToStationId);

public record LineDto(long LineId, string LineName);

public record StationDto(long StationId, string StationName, string? LocationDetail,
                         bool AcceptsItems, IReadOnlyList<string> Lines);