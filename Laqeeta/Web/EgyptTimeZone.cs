namespace Laqeeta.Api.Web;

/// <summary>
/// Converts local date/time input from the Egypt metro operating context to UTC.
/// HTML datetime-local values have no offset, so they must not be interpreted using the server timezone.
/// </summary>
public static class EgyptTimeZone
{
    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public static DateTime ToLocalDateTime(DateTimeOffset utc)
        => TimeZoneInfo.ConvertTime(utc, TimeZone).DateTime;

    public static bool TryConvertLocalToUtc(DateTime localDateTime, out DateTimeOffset utc)
    {
        var local = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        utc = default;

        // Do not silently normalize times skipped or repeated by daylight-saving transitions.
        if (TimeZone.IsInvalidTime(local) || TimeZone.IsAmbiguousTime(local))
            return false;

        var offset = TimeZone.GetUtcOffset(local);
        utc = new DateTimeOffset(local, offset).ToUniversalTime();
        return true;
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
    }
}
