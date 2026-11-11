using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Laqeeta.Infrastructure.Data.Configurations;

public class OwnershipClaimConfiguration : IEntityTypeConfiguration<OwnershipClaim>
{
    public void Configure(EntityTypeBuilder<OwnershipClaim> b)
    {
        b.ToTable("ownership_claims");
        b.HasKey(x => x.ClaimId);

        b.Property(x => x.ClaimStatus).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(ClaimStatus.Pending);
        b.Property(x => x.IdChecked).HasDefaultValue(false);
        b.Property(x => x.SecretAnswerPassed).HasDefaultValue(false);
        b.Property(x => x.ClaimedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // PossibleMatch 1 --- 0..1 OwnershipClaim (match_id is UNIQUE)
        b.HasOne(x => x.Match).WithOne(m => m.Claim)
            .HasForeignKey<OwnershipClaim>(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ClaimedByUser).WithMany(u => u.Claims)
            .HasForeignKey(x => x.ClaimedBy).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.VerifiedByUser).WithMany(u => u.VerifiedClaims)
            .HasForeignKey(x => x.VerifiedBy).OnDelete(DeleteBehavior.Restrict);
    }
}