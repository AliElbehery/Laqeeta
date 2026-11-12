using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Repositories
{
    public class FoundItemRepository : IFoundItemRepository
    {
        private readonly AppDbContext _db;
        public FoundItemRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User> GetEmployeeByIdAsync(long userId, CancellationToken ct = default)
        {
            return await ServiceHelpers.RequireEmployeeAsync(_db, userId, ct);
        }
        public async Task<IReadOnlyList<FoundItem>> GetByStationAsync(long stationId, FoundItemStatus? status = null, CancellationToken ct = default)
        {
            var q = _db.FoundItems.AsNoTracking().Where(i => i.CurrentStationId == stationId);
            if (status is not null) q = q.Where(i => i.ItemStatus == status);

            return await q.Include(i => i.FoundStation)
                          .OrderByDescending(i => i.LoggedAt)
                          .ToListAsync(ct);
        }
        
        public async Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default)
        => await _db.FoundItems.AsNoTracking()
               .Include(i => i.FoundStation).Include(i => i.CurrentStation)
               .FirstOrDefaultAsync(i => i.ItemId == itemId, ct)
           ?? throw new NotFoundException("Item not found.", Err.ItemNotFound);
        
        
   
        public async Task<Station> GetStationByIdAsync(long stationId, CancellationToken ct = default)
        {
            var station = await _db.Stations.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StationId == stationId, ct);
            if (station == null)
                throw new NotFoundException("Station not found.", Err.StationNotFound);
            return station;
        }
        public async Task<Station> GetStationByEmployeeIdAsync(long employeeId, CancellationToken ct = default)
        {
            var employee = await ServiceHelpers.RequireEmployeeAsync(_db, employeeId, ct);
            var station = await _db.Stations.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StationId == employee.StationId, ct);
            if (station == null)
                throw new NotFoundException("Station not found.", Err.StationNotFound);
            return station;
        }

        public async Task<bool> IsItemInStorageAsync(long itemId, CancellationToken ct = default)
        {
           return await _db.Stations.AnyAsync(s => s.StationId == itemId, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
        public async Task AddAsync(FoundItem item, CancellationToken ct = default)
        {
            await _db.FoundItems.AddAsync(item, ct);
        }
    }
}
