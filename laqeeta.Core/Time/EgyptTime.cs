namespace Laqeeta.Core.Time;

/// <summary>
/// The single place that knows the Egypt (Cairo) time zone, shared by the Application layer (matching) and the web layer.
/// Egypt has daylight saving time again (UTC+3 in summer): the clocks go forward at 00:00 on the last Friday of April
/// and back at 24:00 on the last Thursday of October, so never hard-code the offset and never assume a given
/// local time exists.
/// </summary>
public static class EgyptTime
{
    public static TimeZoneInfo Zone { get; } = FindZone();

    /// <summary>
    /// The UTC instant at which a Cairo calendar day starts. Normally that is 00:00 Cairo, but on the day the clocks go
    /// forward 00:00 does not exist (the day starts at 01:00), and <see cref="TimeZoneInfo.ConvertTimeToUtc(DateTime, TimeZoneInfo)"/>
    /// throws for a time that does not exist.
    /// </summary>
    public static DateTimeOffset StartOfDayUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue);          // Kind = Unspecified
        while (Zone.IsInvalidTime(local))
            local = local.AddHours(1);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, Zone), TimeSpan.Zero);
    }

    /// <summary>The Cairo calendar day that a UTC instant falls on (22:30 UTC in winter is already 00:30 of the next day).</summary>
    public static DateOnly DateOf(DateTimeOffset instant)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Zone).DateTime);

    private static TimeZoneInfo FindZone()
    {
        foreach (var id in new[] { "Africa/Cairo", "Egypt Standard Time" })   // IANA id (Linux/macOS), Windows id
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        throw new InvalidOperationException(
            "The Egypt time zone (Africa/Cairo) was not found on this machine. In a Linux container install the 'tzdata' package.");
    }
}
