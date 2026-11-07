using Laqeeta.Core.Enums;

namespace Laqeeta.Core.DTOs;

public record CreateLostReportDto(
    long LineId, string ContactPhone, ItemCategory Category, string Color,
    DateOnly LostDate, TimeOnly? ApproximateTime, string? Direction, string Description);

public record CreateFoundItemDto(
    long FoundStationId, ItemCategory Category, string Color, string Description,
    string? PrivateIdentifyingDetails, DateTime FoundAt);

public record VerifyClaimDto(bool Approve, bool IdChecked, bool SecretAnswerPassed, string? Notes);

public record LineDto(long LineId, string LineName);

public record StationDto(long StationId, string StationName, string? LocationDetail,
                         bool AcceptsItems, IReadOnlyList<string> Lines);
