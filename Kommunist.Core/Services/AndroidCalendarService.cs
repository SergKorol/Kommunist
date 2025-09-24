using System.Reflection;
using Kommunist.Core.Services.Interfaces;
using Plugin.Maui.CalendarStore;   
using IcalEvent = Ical.Net.CalendarComponents.CalendarEvent;
using IcalCalendar = Ical.Net.Calendar;


namespace Kommunist.Core.Services;

public class AndroidCalendarService(ICalendarStore calendarStore) : IAndroidCalendarService
{
    public async Task AddEvents(string icsPath, string? targetCalendarName = null)
    {
        if (!File.Exists(icsPath))
            throw new FileNotFoundException("ICS file not found", icsPath);

        var icsText = await File.ReadAllTextAsync(icsPath);

        var calendar = IcalCalendar.Load(icsText);
        if (calendar?.Events == null || !calendar.Events.Any())
            return;

        var calendars = (await calendarStore.GetCalendars()).ToArray();
        if (calendars == null || calendars.Length == 0)
            throw new InvalidOperationException("Wasn't able to get calendars from the device.");
        
        var targetCalendar = !string.IsNullOrEmpty(targetCalendarName)
            ? calendars.FirstOrDefault(c => string.Equals(c.Name, targetCalendarName, StringComparison.OrdinalIgnoreCase))
            : calendars.First();

        if (targetCalendar == null)
            throw new InvalidOperationException("Wasn't able to find calendar with specified name.");

        var icalEvents = calendar.Events.ToArray();

        foreach (var icalEvt in icalEvents)
        {
            try
            {
                var startDto = ExtractDateTimeOffset(icalEvt, preferStart: true);
                var endDto = ExtractDateTimeOffset(icalEvt, preferStart: false);

                if (!startDto.HasValue)
                {
                    continue;
                }

                endDto ??= startDto.Value.AddHours(1);

                var start = startDto.Value.ToLocalTime().DateTime;
                var end = endDto.Value.ToLocalTime().DateTime;
                
                var isAllDay = SafeGetIsAllDay(icalEvt);

                var alarm = icalEvt.Alarms.FirstOrDefault();
                int? reminderMinutes = null;

                if (alarm is { Trigger.Duration: not null })
                {
                    reminderMinutes = alarm.Trigger.Duration.Value.Minutes;
                }

                reminderMinutes ??= -30;
                
                var reminderTriggerAt = startDto.Value.AddMinutes(reminderMinutes.Value);

                var events = await calendarStore.GetEvents();
                var existingEvent = events.FirstOrDefault(x => x.CalendarId == targetCalendar.Id && x.Title == icalEvt.Summary);
                if (existingEvent != null)
                {
                    await calendarStore.UpdateEvent(
                        eventId: existingEvent.Id, 
                        title: icalEvt.Summary ?? "Event",
                        description: icalEvt.Description ?? string.Empty,
                        location: icalEvt.Location ?? string.Empty,
                        startDateTime: start,
                        endDateTime: end,
                        isAllDay: isAllDay,
                        reminders: [new Reminder(reminderTriggerAt)]);
                }
                else
                {
                    await calendarStore.CreateEvent(
                        targetCalendar.Id,
                        title: icalEvt.Summary ?? "Event",
                        description: icalEvt.Description ?? string.Empty,
                        location: icalEvt.Location ?? string.Empty,
                        startDateTime: start,
                        endDateTime: end,
                        isAllDay: isAllDay,
                        reminders: [new Reminder(reminderTriggerAt)]
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing event '{icalEvt?.Summary}': {ex}");
                throw;
            }
        }
    }

    public async Task<string[]> GetCalendarNames()
    {
        var calendars = (await calendarStore.GetCalendars()).ToArray();
        if (calendars == null || calendars.Length == 0)
            throw new InvalidOperationException("Wasn't able to get calendars from the device.");

        return calendars.Select(c => c.Name).ToArray();
    }
    
    private static DateTimeOffset? ExtractDateTimeOffset(IcalEvent? evt, bool preferStart)
    {
        if (evt == null) return null;

        string[] propNames = preferStart
            ? ["Start", "DtStart", "DTStart", "StartDate", "DtStartUtc"]
            : ["End", "DtEnd", "DTEnd", "EndDate", "DtEndUtc"];

        foreach (var name in propNames)
        {
            var value = TryGetPropertyValue(evt, name);
            var dto = TryConvertToDateTimeOffset(value);
            if (dto.HasValue) return dto;
        }

        var fallback = TryGetPropertyValue(evt, "Start") ?? TryGetPropertyValue(evt, "DtStart") ?? TryGetPropertyValue(evt, "DtStamp");
        return TryConvertToDateTimeOffset(fallback);
    }
    
    private static object? TryGetPropertyValue(object? obj, string propName)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var prop = t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null)
            return prop.GetValue(obj);

        var field = t.GetField(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return field != null ? field.GetValue(obj) : null;
    }

    private static DateTimeOffset? TryConvertToDateTimeOffset(object? value)
    {
        if (value == null) return null;

        var method = value.GetType().GetMethod("AsDateTimeOffset", BindingFlags.Public | BindingFlags.Instance);
        if (method != null)
        {
            var res = method.Invoke(value, null);
            switch (res)
            {
                case DateTimeOffset dto1:
                    return dto1;
                case DateTime dt1:
                    return new DateTimeOffset(dt1);
            }
        }

        method = value.GetType().GetMethod("AsSystemLocal", BindingFlags.Public | BindingFlags.Instance);
        if (method != null)
        {
            var res = method.Invoke(value, null);
            if (res is DateTime dt2) return new DateTimeOffset(dt2);
            var dtProp = res?.GetType().GetProperty("DateTime");
            if (dtProp != null)
            {
                var val = dtProp.GetValue(res);
                if (val is DateTime dt3) return new DateTimeOffset(dt3);
            }
        }

        var valueProp = value.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (valueProp != null)
        {
            var v = valueProp.GetValue(value);
            switch (v)
            {
                case DateTime dtv:
                {
                    if (dtv.Kind == DateTimeKind.Unspecified)
                        dtv = DateTime.SpecifyKind(dtv, DateTimeKind.Local);
                    return new DateTimeOffset(dtv);
                }
                case DateTimeOffset dto:
                    return dto;
            }
        }

        switch (value)
        {
            case DateTime directDt:
            {
                if (directDt.Kind == DateTimeKind.Unspecified)
                    directDt = DateTime.SpecifyKind(directDt, DateTimeKind.Local);
                return new DateTimeOffset(directDt);
            }
            case DateTimeOffset directDto:
                return directDto;
        }

        var s = value.ToString();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTimeOffset.TryParse(s, out var parsedDto)) return parsedDto;
        if (!DateTime.TryParse(s, out var parsedDt)) return null;
        if (parsedDt.Kind == DateTimeKind.Unspecified)
            parsedDt = DateTime.SpecifyKind(parsedDt, DateTimeKind.Local);
        return new DateTimeOffset(parsedDt);

    }

    private static bool SafeGetIsAllDay(IcalEvent? evt)
    {
        try { return evt?.IsAllDay ?? false; }
        catch { return false; }
    }
}