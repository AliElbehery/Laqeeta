using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class FoundItemService(AppDbContext db, IMatchingService matching) : IFoundItemService
{
    public async Task<FoundItem> LogAsync(long employeeId, CreateFoundItemDto dto, CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);

        var heldAt = await db.Stations.FirstAsync(s => s.StationId == employee.StationId, ct);
        if (!heldAt.AcceptsItems)
            throw new BusinessRuleException("Your station does not accept lost items.", Err.StationNoItems);

        if (!await db.Stations.AnyAsync(s => s.StationId == dto.FoundStationId, ct))
            throw new NotFoundException("Found-at station not found.", Err.FoundStationNotFound);

        if (dto.FoundAt > DateTime.UtcNow.AddMinutes(5))
            throw new BusinessRuleException("Found time cannot be in the future.", Err.FoundTimeFuture);

        var item = new FoundItem
        {
            FoundStationId = dto.FoundStationId,
            CurrentStationId = heldAt.StationId,
            LoggedBy = employeeId,
            Category = dto.Category,
            Color = dto.Color.Trim(),
            Description = dto.Description,
            PrivateIdentifyingDetails = dto.PrivateIdentifyingDetails,
            FoundAt = dto.FoundAt
        };
        db.FoundItems.Add(item);
        await db.SaveChangesAsync(ct);

        await matching.GenerateForItemAsync(item.ItemId, ct);
        return item;
    }

    public async Task<IReadOnlyList<FoundItem>> GetByStationAsync(long stationId, FoundItemStatus? status = null,
                                                                  CancellationToken ct = default)
    {
        var q = db.FoundItems.AsNoTracking().Where(i => i.CurrentStationId == stationId);
        if (status is not null) q = q.Where(i => i.ItemStatus == status);

        return await q.Include(i => i.FoundStation)
                      .OrderByDescending(i => i.LoggedAt)
                      .ToListAsync(ct);
    }

    public async Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default)
        => await db.FoundItems.AsNoTracking()
               .Include(i => i.FoundStation).Include(i => i.CurrentStation)
               .FirstOrDefaultAsync(i => i.ItemId == itemId, ct)
           ?? throw new NotFoundException("Item not found.", Err.ItemNotFound);

    public async Task TransferAsync(long itemId, long toStationId, long employeeId, CancellationToken ct = default)
    {
        var employee = await ServiceHelpers.RequireEmployeeAsync(db, employeeId, ct);
        var item = await db.FoundItems.FirstOrDefaultAsync(i => i.ItemId == itemId, ct)
                   ?? throw new NotFoundException("Item not found.", Err.ItemNotFound);

        if (item.CurrentStationId != employee.StationId)
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);
        if (item.ItemStatus != FoundItemStatus.InStorage)
            throw new BusinessRuleException("Only items in storage can be transferred.", Err.TransferNeedsStorage);

        var target = await db.Stations.FirstOrDefaultAsync(s => s.StationId == toStationId, ct)
                     ?? throw new NotFoundException("Target station not found.", Err.TargetStationNotFound);
        if (!target.AcceptsItems) throw new BusinessRuleException("Target station does not accept items.", Err.TargetNoItems);
        if (target.StationId == item.CurrentStationId) throw new BusinessRuleException("Item is already there.", Err.ItemAlreadyThere);

        item.CurrentStationId = target.StationId;
        await db.SaveChangesAsync(ct);
    }
}
