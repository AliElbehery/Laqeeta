using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Laqeeta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class StationService(AppDbContext db) : IStationService
{
    public async Task<IReadOnlyList<LineDto>> GetLinesAsync(CancellationToken ct = default)
        => await db.MetroLines.AsNoTracking()
            .OrderBy(l => l.LineName)
            .Select(l => new LineDto(l.LineId, l.LineName))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StationDto>> GetStationsAsync(long? lineId = null, bool? acceptsItems = null,
                                                                  CancellationToken ct = default)
    {
        var query = db.Stations.AsNoTracking().AsQueryable();

        if (lineId is not null)
            query = query.Where(s => s.StationLines.Any(sl => sl.LineId == lineId));
        if (acceptsItems is not null)
            query = query.Where(s => s.AcceptsItems == acceptsItems);

        var rows = await query.OrderBy(s => s.StationName)
            .Select(s => new
            {
                s.StationId, s.StationName, s.LocationDetail, s.AcceptsItems,
                Lines = s.StationLines.Select(sl => sl.Line.LineName).ToList()
            })
            .ToListAsync(ct);

        return rows.Select(r => new StationDto(r.StationId, r.StationName, r.LocationDetail,
                                               r.AcceptsItems, r.Lines)).ToList();
    }
}
