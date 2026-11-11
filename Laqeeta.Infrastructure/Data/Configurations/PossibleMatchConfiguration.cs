using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class PossibleMatchConfiguration : IEntityTypeConfiguration<PossibleMatch>
{
    public void Configure(EntityTypeBuilder<PossibleMatch> b)
    {
        b.ToTable("possible_matches");
        b.HasKey(x => x.MatchId);

        b.Property(x => x.MatchStatus).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(MatchStatus.Suggested);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasOne(x => x.Report).WithMany(r => r.Matches)
            .HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Item).WithMany(i => i.Matches)
            .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);

        // prevent suggesting the same pair twice
        b.HasIndex(x => new { x.ReportId, x.ItemId }).IsUnique();
    }
}