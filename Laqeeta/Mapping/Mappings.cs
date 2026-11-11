using Laqeeta.Core.DTOs;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Api.Mapping;

/// <summary>Entity -> response DTO. Controllers never return entities (no cycles, no private data leaks).</summary>
public static class Mappings
{
    public static LostReportDto ToDto(this LostReport r) => new(
        r.ReportId, r.UserId, r.LineId, r.Line?.LineName, r.ContactPhone, r.Category, r.Color, r.LostDate,
        r.ApproximateTime, r.Direction, r.Description, r.Status, r.ClosureReason, r.CreatedAt);

    public static ItemPublicDto ToPublicDto(this FoundItem i) => new(
        i.ItemId, i.Category, i.Color, i.Description, i.FoundAt, i.CurrentStationId, i.CurrentStation?.StationName);

    public static FoundItemDto ToStaffDto(this FoundItem i) => new(
        i.ItemId, i.FoundStationId, i.FoundStation?.StationName, i.CurrentStationId, i.CurrentStation?.StationName,
        i.LoggedBy, i.Category, i.Color, i.Description, i.PrivateIdentifyingDetails,
        i.FoundAt, i.LoggedAt, i.ItemStatus, i.ReturnedAt);

    public static PassengerMatchDto ToPassengerDto(this PossibleMatch m)
        => new(m.MatchId, m.ReportId, m.MatchStatus, m.CreatedAt, m.Item.ToPublicDto());

    public static StaffMatchDto ToStaffDto(this PossibleMatch m)
        => new(m.MatchId, m.MatchStatus, m.CreatedAt, m.Report.ToDto(), m.Item.ToStaffDto());

    public static ClaimDto ToDto(this OwnershipClaim c, bool staff = false) => new(
        c.ClaimId, c.MatchId, c.Match?.ReportId, c.Match?.ItemId, c.ClaimStatus,
        c.ClaimedAt, c.VerifiedAt, c.HandedOverAt,
        staff ? c.ClaimedBy : null,
        staff ? c.ClaimedByUser?.FullName : null,
        staff ? c.ClaimedByUser?.Phone : null,
        staff ? c.IdChecked : null,
        staff ? c.SecretAnswerPassed : null,
        staff ? c.VerificationNotes : null);

    /// <param name="lang">"ar" or "en" (see Messages.Normalize); old notifications without a code stay as stored.</param>
    public static NotificationDto ToDto(this Notification n, string lang = "en") => new(
        n.NotificationId, n.Type, Messages.Title(n.Code, n.Title, lang), Messages.Body(n.Code, n.Message, n.Param, lang),
        n.ReportId, n.MatchId, n.IsRead, n.CreatedAt, n.ReadAt);
}