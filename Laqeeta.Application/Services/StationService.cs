using Laqeeta.Application.Abstractions;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;

namespace Laqeeta.Application.Services;

public sealed class StationService(IStationRepository repository) : IStationService
{
    public Task<IReadOnlyList<LineDto>> GetLinesAsync(CancellationToken ct = default)
        => repository.GetLinesAsync(ct);

    public Task<IReadOnlyList<StationDto>> GetStationsAsync(long? lineId = null, bool? acceptsItems = null, CancellationToken ct = default)
        => repository.GetStationsAsync(lineId, acceptsItems, ct);
}