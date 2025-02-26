using System;

namespace Kommunist.Application.Helpers;

public static class DateTimeHelper
{
    public static DateTime ToLocalDateTime(this long date)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(date);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
        var convertedDateTime = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone).DateTime;
        return convertedDateTime;
    }
}