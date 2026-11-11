using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class LostReportConfiguration : IEntityTypeConfiguration<LostReport>
{
    public void Configure(EntityTypeBuilder<LostReport> b)
    {
        b.ToTable("lost_reports");
        b.HasKey(x => x.ReportId);

        b.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Color).IsRequired().HasMaxLength(50);
        b.Property(x => x.LostDate).HasColumnType("date");
        b.Property(x => x.ApproximateTime).HasColumnType("time");
        b.Property(x => x.Direction).HasMaxLength(150);
        b.Property(x => x.Description).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(LostReportStatus.Open);
        b.Property(x => x.ClosureReason).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasOne(x => x.User).WithMany(u => u.LostReports)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Line).WithMany(l => l.LostReports)
            .HasForeignKey(x => x.LineId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.Status, x.Category, x.LineId });
    }
}