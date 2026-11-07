using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class MetroLineConfiguration : IEntityTypeConfiguration<MetroLine>
{
    public void Configure(EntityTypeBuilder<MetroLine> b)
    {
        b.ToTable("metro_lines");
        b.HasKey(x => x.LineId);
        b.Property(x => x.LineName).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.LineName).IsUnique();
    }
}
