using Laqeeta.Core.Enums;
using Laqeeta.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Laqeeta.Infrastructure.Data.Seed;

/// <summary>Idempotent seeder: safe to call on every startup.</summary>
public static class DbSeeder
{
    // DEV ONLY - change/remove before production.
    private const string DevPassword = "Passw0rd!";

    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedMetroAsync(db);
        await SeedUsersAsync(db);
        await SeedSampleDataAsync(db);
    }

    /// <summary>
    /// Creates the first administrator only when explicit credentials are supplied
    /// through user-secrets or environment variables. Never ships a default admin password.
    /// </summary>
    public static async Task SeedAdminAsync(AppDbContext db, IConfiguration configuration)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin)) return;

        var username = configuration["AdminSeed:Username"]?.Trim();
        var email = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"]?.Trim();
        var phone = configuration["AdminSeed:Phone"]?.Trim();

        // No credentials configured: leave admin provisioning to the operator.
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(phone))
            return;

        if (password.Length < 8 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new InvalidOperationException("AdminSeed:Password must be at least 8 characters and contain a letter and a digit.");

        if (await db.Users.AnyAsync(u => u.Username == username || u.Email == email))
            throw new InvalidOperationException("Admin seed username or email already belongs to another account.");

        var admin = new User
        {
            FullName = fullName,
            Username = username,
            Email = email,
            Phone = phone,
            Role = UserRole.Admin,
            IsActive = true
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedMetroAsync(AppDbContext db)
    {
        if (await db.MetroLines.AnyAsync()) return;

        var l1 = new MetroLine { LineName = "Line 1 (Helwan - New El Marg)" };
        var l2 = new MetroLine { LineName = "Line 2 (Shubra El Kheima - Monib)" };
        var l3 = new MetroLine { LineName = "Line 3 (Adly Mansour - Rod El Farag)" };
        db.MetroLines.AddRange(l1, l2, l3);

        Station S(string name, string? detail, bool accepts, params MetroLine[] lines) => new()
        {
            StationName = name,
            LocationDetail = detail,
            AcceptsItems = accepts,
            StationLines = lines.Select(l => new StationLine { Line = l }).ToList()
        };

        db.Stations.AddRange(
            S("Helwan", "Main hall, near ticket office", true, l1),
            S("New El Marg", "Main hall, near ticket office", true, l1),
            S("Sadat", "Interchange - Lost & Found desk, level -1", true, l1, l2),
            S("Al-Shohadaa", "Interchange - Station control room", true, l1, l2),
            S("Nasser", "Interchange - Station control room", true, l1, l3),
            S("Attaba", "Interchange - Station control room", true, l2, l3),
            S("Cairo University", null, false, l2),
            S("Shubra El Kheima", "Main hall", true, l2),
            S("Adly Mansour", "Main hall", true, l3),
            S("Kit Kat", null, false, l3));

        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();
        var sadat = await db.Stations.FirstAsync(s => s.StationName == "Sadat");
        var attaba = await db.Stations.FirstAsync(s => s.StationName == "Attaba");

        User Make(string fullName, string username, string email, string phone,
                  UserRole role, Station? station)
        {
            var u = new User
            {
                FullName = fullName,
                Username = username,
                Email = email,
                Phone = phone,
                Role = role,
                Station = station,
                IsActive = true
            };
            u.PasswordHash = hasher.HashPassword(u, DevPassword);
            return u;
        }

        db.Users.AddRange(
            Make("Ahmed Employee (Sadat)", "emp.sadat", "emp.sadat@metro.local", "01000000001", UserRole.Employee, sadat),
            Make("Mona Employee (Attaba)", "emp.attaba", "emp.attaba@metro.local", "01000000002", UserRole.Employee, attaba),
            Make("Omar Passenger", "omar", "omar@example.com", "01000000010", UserRole.User, null),
            Make("Sara Passenger", "sara", "sara@example.com", "01000000011", UserRole.User, null));

        await db.SaveChangesAsync();
    }

    private static async Task SeedSampleDataAsync(AppDbContext db)
    {
        if (await db.LostReports.AnyAsync()) return;

        var omar = await db.Users.FirstAsync(u => u.Username == "omar");
        var emp = await db.Users.FirstAsync(u => u.Username == "emp.sadat");
        var sadat = await db.Stations.FirstAsync(s => s.StationName == "Sadat");
        var line1 = await db.MetroLines.FirstAsync(l => l.LineName.StartsWith("Line 1"));

        var report = new LostReport
        {
            User = omar,
            Line = line1,
            ContactPhone = omar.Phone,
            Category = ItemCategory.Wallet,
            Color = "Black",
            LostDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            ApproximateTime = new TimeOnly(8, 30),
            Direction = "Towards Helwan",
            Description = "Black leather wallet with a few cards inside."
        };

        var item = new FoundItem
        {
            FoundStation = sadat,
            CurrentStation = sadat,
            LoggedByUser = emp,
            Category = ItemCategory.Wallet,
            Color = "Black",
            Description = "Black leather wallet found on the platform.",
            PrivateIdentifyingDetails = "Contains a student card and a blue library card.",
            FoundAt = DateTime.UtcNow.AddHours(-20)
        };

        var match = new PossibleMatch { Report = report, Item = item };

        db.Notifications.Add(new Notification
        {
            User = omar,
            Report = report,
            Match = match,
            Type = NotificationType.MatchFound,
            Title = "Possible match for your lost item",
            Message = "An item similar to your report was found. Check your matches.",
            Code = Laqeeta.Core.Localization.NotifCode.MatchFoundForReport
        });

        await db.SaveChangesAsync();
    }
}