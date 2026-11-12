using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Abstractions;

public interface IFoundItemRepository
{
    Task<IReadOnlyList<FoundItem>> GetByStationAsync(
        long stationId, FoundItemStatus? status = null, CancellationToken ct = default);

    // Read-only query for displaying item details.
    Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default);

    // Tracked query for use cases that modify an item.
    Task<FoundItem> GetForUpdateAsync(long itemId, CancellationToken ct = default);

    Task<Station> GetStationByIdAsync(long stationId, CancellationToken ct = default);
    Task<bool> StationExistsAsync(long stationId, CancellationToken ct = default);
    Task AddAsync(FoundItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<User> GetEmployeeByIdAsync(long userId, CancellationToken ct = default);
}
