using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("notifications");
        b.HasKey(x => x.NotificationId);

        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Message).IsRequired();
        b.Property(x => x.Code).HasMaxLength(60);
        b.Property(x => x.Param).HasMaxLength(200);
        b.Property(x => x.IsRead).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasOne(x => x.User).WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Report).WithMany(r => r.Notifications)
            .HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Match).WithMany(m => m.Notifications)
            .HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.UserId, x.IsRead });
    }
}
