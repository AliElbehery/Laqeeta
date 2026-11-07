using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public interface IMatchingService
{
    /// <summary>Finds found-items that look like this report and stores them as suggested matches.</summary>
    Task<IReadOnlyList<PossibleMatch>> GenerateForReportAsync(long reportId, CancellationToken ct = default);

    /// <summary>Finds open reports that look like this found item.</summary>
    Task<IReadOnlyList<PossibleMatch>> GenerateForItemAsync(long itemId, CancellationToken ct = default);

    Task<IReadOnlyList<PossibleMatch>> GetForReportAsync(long reportId, long userId, CancellationToken ct = default);
    Task ConfirmAsync(long matchId, long employeeId, CancellationToken ct = default);
    Task RejectAsync(long matchId, long employeeId, CancellationToken ct = default);
}
