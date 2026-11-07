using Laqeeta.Core.Models;
using Laqeeta.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class FoundItemConfiguration : IEntityTypeConfiguration<FoundItem>
{
    public void Configure(EntityTypeBuilder<FoundItem> b)
    {
        b.ToTable("found_items");
        b.HasKey(x => x.ItemId);

        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Color).IsRequired().HasMaxLength(50);
        b.Property(x => x.Description).IsRequired();
        b.Property(x => x.PrivateIdentifyingDetails);
        b.Property(x => x.LoggedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.ItemStatus).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(FoundItemStatus.InStorage);

        // Station -> FoundItem (found at)
        b.HasOne(x => x.FoundStation).WithMany(s => s.ItemsFoundHere)
            .HasForeignKey(x => x.FoundStationId).OnDelete(DeleteBehavior.Restrict);

        // Station -> FoundItem (currently held at)
        b.HasOne(x => x.CurrentStation).WithMany(s => s.ItemsHeldHere)
            .HasForeignKey(x => x.CurrentStationId).OnDelete(DeleteBehavior.Restrict);

        // User (employee) -> FoundItem (logged by)
        b.HasOne(x => x.LoggedByUser).WithMany(u => u.LoggedItems)
            .HasForeignKey(x => x.LoggedBy).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ItemStatus, x.Category, x.CurrentStationId });
    }
}
