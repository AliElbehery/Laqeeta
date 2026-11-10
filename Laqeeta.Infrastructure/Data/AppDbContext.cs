using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;
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

        // Optimistic concurrency for every entity that has a ConcurrencyStamp (see IHasConcurrencyStamp).
        // A Guid that the application sets (instead of SQL Server's rowversion) works the same on SQL Server and SQLite.
        foreach (var entity in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(IHasConcurrencyStamp).IsAssignableFrom(t.ClrType)).ToList())
            modelBuilder.Entity(entity.ClrType)
                .Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                .IsConcurrencyToken();
    }

    // ------------------------------------------------------------------ optimistic concurrency
    // SaveChanges()/SaveChangesAsync() with no arguments call these two, so this covers every save.

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampChangedRows();
        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConcurrencyConflict();
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampChangedRows();
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConcurrencyConflict();
        }
    }

    /// <summary>New stamp for every inserted/updated row. The OLD value stays in OriginalValues and goes into the WHERE.</summary>
    private void StampChangedRows()
    {
        ChangeTracker.DetectChanges();   // so "Modified" is up to date before we look at it
        foreach (var entry in ChangeTracker.Entries<IHasConcurrencyStamp>())
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.ConcurrencyStamp = Guid.NewGuid();
    }

    /// <summary>A 409 (API) / flash error (website) instead of a 500. Nothing was written: the save is one transaction.</summary>
    private static ConflictException ConcurrencyConflict()
        => new("Someone else changed the same data at the same time. Refresh and try again.", Err.ConcurrencyConflict);
}
