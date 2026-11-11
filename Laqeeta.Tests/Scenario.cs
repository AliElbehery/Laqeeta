using Laqeeta.Application.Services;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Repositories;
using Laqeeta.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Laqeeta.Tests;

/// <summary>
/// The real model, but on SQLite instead of SQL Server: the two SQL Server specifics
/// (SYSUTCDATETIME() defaults and "date"/"time" column types) are replaced.
/// SQLite enforces foreign keys and unique indexes, so the tests behave like the real database.
/// </summary>
public class SqliteAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    /// <summary>
    /// Test hook for race conditions: runs once, right before this context's next save. A test uses it to let
    /// "another request" finish first, i.e. the service has already read its data when the competing change lands.
    /// </summary>
    public Func<Task>? BeforeNextSave { get; set; }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (BeforeNextSave is { } competingRequest)
        {
            BeforeNextSave = null;
            await competingRequest();
        }
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
            {
                if (property.GetDefaultValueSql() is not null)
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP");
                if (property.FindAnnotation(RelationalAnnotationNames.ColumnType) is not null)
                    property.SetColumnType(null);
            }
    }
}

public sealed class FakeTokenService : ITokenService
{
    public TokenResult Create(User user) => new($"fake-token-{user.UserId}", DateTime.UtcNow.AddHours(1));
}

/// <summary>
/// A fresh in-memory database with a small metro network and a few users.
/// Every service call gets its own DbContext, exactly like a web request.
///
///   Line 1: Sadat, Attaba, Maadi (Maadi does NOT accept items)
///   Line 2: Attaba, Shubra
///   Passengers: omar, sara            Employees: emp.sadat, emp.attaba, emp.maadi
/// </summary>
public sealed class Scenario : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public long Line1Id { get; private set; }
    public long Line2Id { get; private set; }
    public long SadatId { get; private set; }
    public long AttabaId { get; private set; }
    public long ShubraId { get; private set; }
    public long MaadiId { get; private set; }
    public long OmarId { get; private set; }
    public long SaraId { get; private set; }
    public long EmpSadatId { get; private set; }
    public long EmpAttabaId { get; private set; }
    public long EmpMaadiId { get; private set; }

    public Scenario()
    {
        _connection.Open();
        using var db = NewDb();
        db.Database.EnsureCreated();
        Seed(db);
    }

    public SqliteAppDbContext NewDb()
        => new SqliteAppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();

    // ----------------------------------------------------------------- services (fresh context each)
    public MatchingService Matching() => new(NewDb());

    public ClaimService Claims() => new(NewDb());

    public NotificationService Notifications() => new(NewDb());

    public AuthService Auth() => new(NewDb(), new FakeTokenService());

    public LostReportService Reports()
    {
        var db = NewDb();
        return new LostReportService(new LostReportRepository(db), new MatchingService(db));
    }

    public FoundItemService Items()
    {
        var db = NewDb();
        return new FoundItemService(db, new MatchingService(db));
    }

    /// <summary>Runs a query on a fresh context (what a test asserts against).</summary>
    public async Task<T> Read<T>(Func<AppDbContext, Task<T>> query)
    {
        using var db = NewDb();
        return await query(db);
    }

    // ----------------------------------------------------------------- arrange helpers
    public async Task<LostReport> AddReportAsync(
        long? userId = null, ItemCategory category = ItemCategory.Wallet, string color = "Black",
        long? lineId = null, DateOnly? lostDate = null, LostReportStatus status = LostReportStatus.Open)
    {
        using var db = NewDb();
        var report = new LostReport
        {
            UserId = userId ?? OmarId,
            LineId = lineId ?? Line1Id,
            ContactPhone = "01000000001",
            Category = category,
            Color = color,
            LostDate = lostDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Description = "test report",
            Status = status
        };
        db.LostReports.Add(report);
        await db.SaveChangesAsync();
        return report;
    }

    public async Task<FoundItem> AddItemAsync(
        long? heldAtId = null, long? foundStationId = null, ItemCategory category = ItemCategory.Wallet,
        string color = "Black", FoundItemStatus status = FoundItemStatus.InStorage, DateTime? foundAt = null,
        string? privateDetails = "Blue card inside")
    {
        using var db = NewDb();
        var heldAt = heldAtId ?? SadatId;
        var item = new FoundItem
        {
            FoundStationId = foundStationId ?? heldAt,
            CurrentStationId = heldAt,
            LoggedBy = EmpSadatId,
            Category = category,
            Color = color,
            Description = "test item",
            PrivateIdentifyingDetails = privateDetails,
            FoundAt = foundAt ?? DateTime.UtcNow.AddMinutes(-10),
            ItemStatus = status
        };
        db.FoundItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<PossibleMatch> AddMatchAsync(long reportId, long itemId, MatchStatus status = MatchStatus.Suggested)
    {
        using var db = NewDb();
        var match = new PossibleMatch { ReportId = reportId, ItemId = itemId, MatchStatus = status };
        db.PossibleMatches.Add(match);
        await db.SaveChangesAsync();
        return match;
    }

    public async Task<OwnershipClaim> AddClaimAsync(long matchId, long userId, ClaimStatus status = ClaimStatus.Pending)
    {
        using var db = NewDb();
        var claim = new OwnershipClaim { MatchId = matchId, ClaimedBy = userId, ClaimStatus = status };
        db.OwnershipClaims.Add(claim);
        await db.SaveChangesAsync();
        return claim;
    }

    /// <summary>Open report + item in storage at Sadat + a suggested match between them.</summary>
    public async Task<(LostReport Report, FoundItem Item, PossibleMatch Match)> SuggestedMatchAsync(long? userId = null)
    {
        var report = await AddReportAsync(userId);
        var item = await AddItemAsync();
        var match = await AddMatchAsync(report.ReportId, item.ItemId);
        return (report, item, match);
    }

    /// <summary>Same as <see cref="SuggestedMatchAsync"/> but already confirmed (report Matched, item Matched).</summary>
    public async Task<(LostReport Report, FoundItem Item, PossibleMatch Match)> ConfirmedMatchAsync(long? userId = null)
    {
        var report = await AddReportAsync(userId, status: LostReportStatus.Matched);
        var item = await AddItemAsync(status: FoundItemStatus.Matched);
        var match = await AddMatchAsync(report.ReportId, item.ItemId, MatchStatus.Confirmed);
        return (report, item, match);
    }

    /// <summary>Two steps on purpose: a new row with IsActive=false would get the database default (true).</summary>
    public async Task SetActiveAsync(long userId, bool active)
    {
        using var db = NewDb();
        var user = await db.Users.FirstAsync(u => u.UserId == userId);
        user.IsActive = active;
        await db.SaveChangesAsync();
    }

    // ----------------------------------------------------------------- seed
    private void Seed(AppDbContext db)
    {
        var line1 = new MetroLine { LineName = "Line 1" };
        var line2 = new MetroLine { LineName = "Line 2" };

        var sadat = new Station { StationName = "Sadat", AcceptsItems = true, StationLines = { new StationLine { Line = line1 } } };
        var attaba = new Station
        {
            StationName = "Attaba",
            AcceptsItems = true,
            StationLines = { new StationLine { Line = line1 }, new StationLine { Line = line2 } }
        };
        var shubra = new Station { StationName = "Shubra", AcceptsItems = true, StationLines = { new StationLine { Line = line2 } } };
        var maadi = new Station { StationName = "Maadi", AcceptsItems = false, StationLines = { new StationLine { Line = line1 } } };

        static User U(string username, UserRole role, Station? station = null) => new()
        {
            FullName = username,
            Username = username,
            Email = $"{username}@test.com",
            Phone = "01000000000",
            PasswordHash = "not-a-real-hash",
            Role = role,
            Station = station
        };

        var omar = U("omar", UserRole.User);
        var sara = U("sara", UserRole.User);
        var eSadat = U("emp.sadat", UserRole.Employee, sadat);
        var eAttaba = U("emp.attaba", UserRole.Employee, attaba);
        var eMaadi = U("emp.maadi", UserRole.Employee, maadi);

        db.AddRange(line1, line2, sadat, attaba, shubra, maadi, omar, sara, eSadat, eAttaba, eMaadi);
        db.SaveChanges();

        Line1Id = line1.LineId; Line2Id = line2.LineId;
        SadatId = sadat.StationId; AttabaId = attaba.StationId; ShubraId = shubra.StationId; MaadiId = maadi.StationId;
        OmarId = omar.UserId; SaraId = sara.UserId;
        EmpSadatId = eSadat.UserId; EmpAttabaId = eAttaba.UserId; EmpMaadiId = eMaadi.UserId;
    }
}