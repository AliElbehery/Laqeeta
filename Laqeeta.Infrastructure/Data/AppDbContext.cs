using Laqeeta.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MetroLine> MetroLines => Set<MetroLine>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<StationLine> StationLines => Set<StationLine>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LostReport> LostReports => Set<LostReport>();
    public DbSet<FoundItem> FoundItems => Set<FoundItem>();
    public DbSet<PossibleMatch> PossibleMatches => Set<PossibleMatch>();
    public DbSet<OwnershipClaim> OwnershipClaims => Set<OwnershipClaim>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
