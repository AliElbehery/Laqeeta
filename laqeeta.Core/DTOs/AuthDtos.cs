using System.ComponentModel.DataAnnotations;
using Laqeeta.Core.Enums;

namespace Laqeeta.Core.DTOs;

public record RegisterDto(
    [Required, StringLength(150, MinimumLength = 2)] string FullName,
    [Required, StringLength(50, MinimumLength = 3), RegularExpression(@"^[A-Za-z0-9._-]+$",
        ErrorMessage = "Username may contain letters, digits, '.', '_' and '-' only.")] string Username,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Invalid phone number.")] string Phone,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginDto(
    [Required] string UsernameOrEmail,
    [Required] string Password);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

public record CreateEmployeeDto(
    [Required, StringLength(150, MinimumLength = 2)] string FullName,
    [Required, StringLength(50, MinimumLength = 3), RegularExpression(@"^[A-Za-z0-9._-]+$")] string Username,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, RegularExpression(@"^\+?[0-9]{8,15}$")] string Phone,
    [Required, StringLength(100, MinimumLength = 8)] string Password,
    [Range(1, long.MaxValue)] long StationId);

public record UserDto(long UserId, string FullName, string Username, string Email, string Phone,
                      UserRole Role, long? StationId);

public record AuthResultDto(string Token, DateTime ExpiresAt, UserDto User);