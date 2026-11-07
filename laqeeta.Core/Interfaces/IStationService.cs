using Laqeeta.Core.DTOs;

namespace Laqeeta.Core.Interfaces;

public interface IStationService
{
    Task<IReadOnlyList<LineDto>> GetLinesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<StationDto>> GetStationsAsync(long? lineId = null, bool? acceptsItems = null, CancellationToken ct = default);
}
