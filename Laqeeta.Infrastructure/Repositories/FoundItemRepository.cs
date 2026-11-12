using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories;

public class FoundItemRepository(AppDbContext db) : IFoundItemRepository
{
    public Task<User> GetEmployeeByIdAsync(long userId, CancellationToken ct = default)
        => ServiceHelpers.RequireEmployeeAsync(db, userId, ct);

    public async Task<IReadOnlyList<FoundItem>> GetByStationAsync(
        long stationId, FoundItemStatus? status = null, CancellationToken ct = default)
    {
        var query = db.FoundItems.AsNoTracking()
            .Where(item => item.CurrentStationId == stationId);

        if (status is not null)
            query = query.Where(item => item.ItemStatus == status);

        return await query
            .Include(item => item.FoundStation)
            .OrderByDescending(item => item.LoggedAt)
            .ThenByDescending(item => item.ItemId)
            .ToListAsync(ct);
    }

    public async Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default)
        => await db.FoundItems.AsNoTracking()
            .Include(item => item.FoundStation)
            .Include(item => item.CurrentStation)
            .FirstOrDefaultAsync(item => item.ItemId == itemId, ct)
           ?? throw new NotFoundException("Item not found.", Err.ItemNotFound);

    public async Task<FoundItem> GetForUpdateAsync(long itemId, CancellationToken ct = default)
        => await db.FoundItems
            .Include(item => item.FoundStation)
            .Include(item => item.CurrentStation)
            .FirstOrDefaultAsync(item => item.ItemId == itemId, ct)
           ?? throw new NotFoundException("Item not found.", Err.ItemNotFound);

    public async Task<Station> GetStationByIdAsync(long stationId, CancellationToken ct = default)
        => await db.Stations.AsNoTracking()
            .FirstOrDefaultAsync(station => station.StationId == stationId, ct)
           ?? throw new NotFoundException("Station not found.", Err.StationNotFound);

    public Task<bool> StationExistsAsync(long stationId, CancellationToken ct = default)
        => db.Stations.AnyAsync(station => station.StationId == stationId, ct);

    public Task AddAsync(FoundItem item, CancellationToken ct = default)
        => db.FoundItems.AddAsync(item, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
