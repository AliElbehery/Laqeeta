using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class StationLineConfiguration : IEntityTypeConfiguration<StationLine>
{
    public void Configure(EntityTypeBuilder<StationLine> b)
    {
        b.ToTable("station_lines");
        b.HasKey(x => new { x.StationId, x.LineId });

        b.HasOne(x => x.Station).WithMany(s => s.StationLines)
            .HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Line).WithMany(l => l.StationLines)
            .HasForeignKey(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}
