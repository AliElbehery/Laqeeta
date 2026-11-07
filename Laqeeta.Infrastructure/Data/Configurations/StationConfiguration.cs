using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> b)
    {
        b.ToTable("stations");
        b.HasKey(x => x.StationId);
        b.Property(x => x.StationName).IsRequired().HasMaxLength(150);
        b.Property(x => x.LocationDetail);
        b.Property(x => x.AcceptsItems).HasDefaultValue(false);
    }
}
