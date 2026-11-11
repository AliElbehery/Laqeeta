using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public interface IFoundItemService
{
    /// <summary>Employee logs an item; it is held at the employee's own station.</summary>
    Task<FoundItem> LogAsync(long employeeId, CreateFoundItemDto dto, CancellationToken ct = default);

    Task<IReadOnlyList<FoundItem>> GetByStationAsync(long stationId, FoundItemStatus? status = null, CancellationToken ct = default);

    Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default);

    Task TransferAsync(long itemId, long toStationId, long employeeId, CancellationToken ct = default);
}