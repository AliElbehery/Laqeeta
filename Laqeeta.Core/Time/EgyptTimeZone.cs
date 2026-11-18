namespace Laqeeta.Core.Time;

public static class EgyptTimeZone
{
    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public static DateTime ToLocalDateTime(DateTimeOffset utc)
        => TimeZoneInfo.ConvertTime(utc, TimeZone).DateTime;

    public static DateOnly ToLocalDate(DateTimeOffset utc)
        => DateOnly.FromDateTime(ToLocalDateTime(utc));

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
