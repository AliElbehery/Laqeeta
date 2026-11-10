using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

/// <summary>
/// Persistence operations required by the lost-report use cases.
/// The application layer depends on this abstraction, not on EF Core.
/// </summary>
public interface ILostReportRepository
{
    Task<User?> GetUserAsync(long userId, CancellationToken ct = default);
    Task<bool> MetroLineExistsAsync(long lineId, CancellationToken ct = default);
    Task AddAsync(LostReport report, CancellationToken ct = default);
    Task<IReadOnlyList<LostReport>> GetByUserAsync(long userId, CancellationToken ct = default);
    Task<LostReport?> GetByIdWithLineAsync(long reportId, CancellationToken ct = default);
    Task<LostReport?> GetForCancellationAsync(long reportId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
