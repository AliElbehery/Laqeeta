using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

public interface IMatchingRepository
{
    Task<LostReport?> GetReportAsync(long reportId, CancellationToken ct);
    Task<IReadOnlyList<FoundItem>> GetCandidatesForReportAsync(LostReport report, DateTimeOffset lostFrom, CancellationToken ct);
    Task<FoundItem?> GetItemWithStationAsync(long itemId, CancellationToken ct);
    Task<IReadOnlyList<LostReport>> GetCandidatesForItemAsync(FoundItem item, IReadOnlyCollection<long> lineIds, DateOnly foundDate, CancellationToken ct);
    void AddMatch(PossibleMatch match);
    void AddNotification(Notification notification);
    Task<LostReport?> GetReportForUserAsync(long reportId, CancellationToken ct);
    Task<IReadOnlyList<PossibleMatch>> GetMatchesForReportAsync(long reportId, CancellationToken ct);
    Task<IReadOnlyList<PossibleMatch>> GetMatchesForStationAsync(long stationId, MatchStatus? status, CancellationToken ct);
    Task<User?> GetUserAsync(long userId, CancellationToken ct);
    Task<PossibleMatch?> GetMatchForUpdateAsync(long matchId, CancellationToken ct);
    void MarkMatchStatusModified(PossibleMatch match);
    Task SaveChangesAsync(CancellationToken ct);
    void ClearTracking();
}