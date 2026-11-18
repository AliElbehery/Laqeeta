using Laqeeta.Core.DTOs;

namespace Laqeeta.Application.Abstractions;

public interface IStationRepository
{
    Task<IReadOnlyList<LineDto>> GetLinesAsync(CancellationToken ct);
    Task<IReadOnlyList<StationDto>> GetStationsAsync(long? lineId, bool? acceptsItems, CancellationToken ct);
}