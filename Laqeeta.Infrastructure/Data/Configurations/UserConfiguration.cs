using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.UserId);

        b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        b.Property(x => x.Username).IsRequired().HasMaxLength(50);
        b.Property(x => x.Email).IsRequired().HasMaxLength(256);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(20);
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(UserRole.User);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasIndex(x => x.Username).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();

        // Station 1 --- 0..* Users ("works at")
        b.HasOne(x => x.Station).WithMany(s => s.Employees)
            .HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
    }
}