using Laqeeta.Core.DTOs;
using Laqeeta.Core.Enums;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Localization;
using Laqeeta.Core.Models;
using Laqeeta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Services;

public class AuthService(AppDbContext db, ITokenService tokens) : IAuthService
{
    private static readonly PasswordHasher<User> Hasher = new();

    // Used to burn the same CPU time when the user does not exist (avoids user enumeration by timing).
    private static readonly string DummyHash = Hasher.HashPassword(new User(), "Dummy-Password-1");

    private const string InvalidCredentials = "Invalid username/email or password.";

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var user = await CreateUserAsync(dto.FullName, dto.Username, dto.Email, dto.Phone, dto.Password,
                                         UserRole.User, stationId: null, ct);
        return BuildResult(user);
    }

    public async Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default)
    {
        if (!await db.Stations.AnyAsync(s => s.StationId == dto.StationId, ct))
            throw new NotFoundException("Station not found.", Err.StationNotFound);

        var user = await CreateUserAsync(dto.FullName, dto.Username, dto.Email, dto.Phone, dto.Password,
                                         UserRole.Employee, dto.StationId, ct);
        return ToDto(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var id = dto.UsernameOrEmail.Trim();
        var email = id.ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == id || u.Email == email, ct);

        if (user is null)
        {
            Hasher.VerifyHashedPassword(new User(), DummyHash, dto.Password);
            throw new UnauthorizedException(InvalidCredentials, Err.InvalidCredentials);
        }

        var result = Hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(InvalidCredentials, Err.InvalidCredentials);

        if (!user.IsActive)
            throw new ForbiddenException("Account is disabled.", Err.AccountDisabled);

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = Hasher.HashPassword(user, dto.Password);
            await db.SaveChangesAsync(ct);
        }

        return BuildResult(user);
    }

    public async Task<UserDto> GetProfileAsync(long userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, ct)
                   ?? throw new NotFoundException("User not found.", Err.UserNotFound);
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct)
                   ?? throw new NotFoundException("User not found.", Err.UserNotFound);

        if (Hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword) == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Current password is incorrect.", Err.CurrentPasswordWrong);

        EnsureStrongPassword(dto.NewPassword);
        if (dto.NewPassword == dto.CurrentPassword)
            throw new BusinessRuleException("New password must be different from the current one.", Err.PasswordSame);

        user.PasswordHash = Hasher.HashPassword(user, dto.NewPassword);
        await db.SaveChangesAsync(ct);
    }

    // ---------------------------------------------------------------- helpers

    private async Task<User> CreateUserAsync(string fullName, string username, string email, string phone,
                                             string password, UserRole role, long? stationId, CancellationToken ct)
    {
        username = username.Trim();
        email = email.Trim().ToLowerInvariant();
        EnsureStrongPassword(password);

        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            throw new ConflictException("Username is already taken.", Err.UsernameTaken);
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Email is already registered.", Err.EmailTaken);

        var user = new User
        {
            FullName = fullName.Trim(),
            Username = username,
            Email = email,
            Phone = phone.Trim(),
            Role = role,
            StationId = stationId,
            IsActive = true
        };
        user.PasswordHash = Hasher.HashPassword(user, password);

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // unique index hit by a concurrent request
            throw new ConflictException("Username or email is already registered.", Err.UserExists);
        }
        return user;
    }

    private static void EnsureStrongPassword(string password)
    {
        if (password.Length < 8 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new BusinessRuleException("Password must be at least 8 characters and contain a letter and a digit.", Err.PasswordWeak);
    }

    private AuthResultDto BuildResult(User user)
    {
        var t = tokens.Create(user);
        return new AuthResultDto(t.Token, t.ExpiresAt, ToDto(user));
    }

    private static UserDto ToDto(User u)
        => new(u.UserId, u.FullName, u.Username, u.Email, u.Phone, u.Role, u.StationId);
}