using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;

namespace Laqeeta.Application.Services;

public class FoundItemService(IFoundItemRepository repository, IMatchingService matching) : IFoundItemService
{
    public async Task<FoundItem> LogAsync(
        long employeeId, CreateFoundItemDto dto, CancellationToken ct = default)
    {
        var employee = await repository.GetEmployeeByIdAsync(employeeId, ct);
        var heldAt = await repository.GetStationByIdAsync(employee.StationId!.Value, ct);

        if (!heldAt.AcceptsItems)
            throw new BusinessRuleException("Your station does not accept lost items.", Err.StationNoItems);

        if (!await repository.StationExistsAsync(dto.FoundStationId, ct))
            throw new NotFoundException("Found-at station not found.", Err.FoundStationNotFound);

        if (dto.FoundAt > DateTimeOffset.UtcNow.AddMinutes(5))
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
            FoundAt = dto.FoundAt.ToUniversalTime()
        };

        await repository.AddAsync(item, ct);
        await repository.SaveChangesAsync(ct);
        await matching.GenerateForItemAsync(item.ItemId, ct);

        return item;
    }

    public Task<IReadOnlyList<FoundItem>> GetByStationAsync(
        long stationId, FoundItemStatus? status = null, CancellationToken ct = default)
        => repository.GetByStationAsync(stationId, status, ct);

    public Task<FoundItem> GetByIdAsync(long itemId, CancellationToken ct = default)
        => repository.GetByIdAsync(itemId, ct);

    public async Task TransferAsync(
        long itemId, long toStationId, long employeeId, CancellationToken ct = default)
    {
        var employee = await repository.GetEmployeeByIdAsync(employeeId, ct);
        var item = await repository.GetForUpdateAsync(itemId, ct);

        if (item.CurrentStationId != employee.StationId)
            throw new ForbiddenException("The item is held at a different station.", Err.WrongStation);

        if (item.ItemStatus != FoundItemStatus.InStorage)
            throw new BusinessRuleException("Only items in storage can be transferred.", Err.TransferNeedsStorage);

        var target = await repository.GetStationByIdAsync(toStationId, ct);
        if (!target.AcceptsItems)
            throw new BusinessRuleException("Target station does not accept items.", Err.TargetNoItems);

        if (target.StationId == item.CurrentStationId)
            throw new BusinessRuleException("Item is already there.", Err.ItemAlreadyThere);

        item.CurrentStationId = target.StationId;
        await repository.SaveChangesAsync(ct);
    }
}
