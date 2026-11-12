namespace Laqeeta.Core.DTOs;

public sealed class AdminDashboardDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int TotalPassengers { get; init; }
    public int TotalStations { get; init; }
    public int TotalLostReports { get; init; }
    public int OpenLostReports { get; init; }
    public int TotalFoundItems { get; init; }
    public int ItemsInStorage { get; init; }
    public int PendingClaims { get; init; }
    public int SuggestedMatches { get; init; }
}

public sealed record AdminEmployeeDto(
    long UserId,
    string FullName,
    string Username,
    string Email,
    string Phone,
    string? StationName,
    bool IsActive,
    DateTime CreatedAt);

public enum ToggleEmployeeResult
{
    Updated,
    NotFound,
    CannotChangeOwnAccount
}
