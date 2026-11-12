using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;


namespace Laqeeta.Application.Abstractions
{
    public interface IFoundItemRepository
    {
        Task<IReadOnlyList<FoundItem>> GetByStationAsync(long stationId, FoundItemStatus? status = null, CancellationToken ct = default);
        Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default);
        Task<Station> GetStationByIdAsync(long stationId, CancellationToken ct = default);
        Task<bool> IsItemInStorageAsync(long itemId, CancellationToken ct = default);
        Task AddAsync(FoundItem item, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<Station> GetStationByEmployeeIdAsync(long employeeId, CancellationToken ct = default);
        Task<User> GetEmployeeByIdAsync(long userId, CancellationToken ct = default);
    }
}
