using System.Reflection;
using FluentAssertions;
using Kommunist.Core.Services;
using Moq;
using Plugin.Maui.CalendarStore;

namespace Kommunist.Tests.Services;

public class AndroidCalendarServiceTests
{
    [Fact]
    public async Task AddEvents_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Strict);

        var sut = new AndroidCalendarService(store.Object);

        Func<Task> act = () => sut.AddEvents("Z:\\does-not-exist\\file.ics");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task AddEvents_WhenNoDeviceCalendars_ThrowsInvalidOperationException()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Strict);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(Array.Empty<Calendar>());

        var sut = new AndroidCalendarService(store.Object);
        var icsPath = CreateTempIcs(
            VEvent("2025-01-01T10:00:00Z", "2025-01-01T11:00:00Z", "Any Event")
        ); // ensure there is at least one event

        try
        {
            Func<Task> act = () => sut.AddEvents(icsPath);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Wasn't able to get calendars from the device.");
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WhenTargetCalendarNameNotFound_ThrowsInvalidOperationException()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Work"),
            NewCalendar("calB", "Personal")
        });

        var sut = new AndroidCalendarService(store.Object);
        var icsPath = CreateTempIcs(VEvent("20250101T100000Z", "20250101T110000Z", "Some Event"));

        try
        {
            Func<Task> act = () => sut.AddEvents(icsPath, targetCalendarName: "NonExistingName");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Не вдалося знайти календар з вказаною назвою.");
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WhenIcsHasNoEvents_DoesNotCreateOrUpdate()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Default")
        });
        store.Setup(s => s.GetEvents(
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>()
            ))
            .ReturnsAsync(Array.Empty<CalendarEvent>());

        var sut = new AndroidCalendarService(store.Object);
        var icsPath = CreateTempIcs(); // No VEVENT entries

        try
        {
            await sut.AddEvents(icsPath);

            store.Invocations.Count(i => i.Method.Name == "CreateEvent").Should().Be(0);
            store.Invocations.Count(i => i.Method.Name == "UpdateEvent").Should().Be(0);
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WhenNewEvent_ShouldCreateWithExpectedFields_AndDefaultReminder()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        var calendars = new[]
        {
            NewCalendar("calA", "Work"),
            NewCalendar("calB", "Personal")
        };
        store.Setup(s => s.GetCalendars()).ReturnsAsync(calendars);
        store.Setup(s => s.GetEvents(
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>()
            ))
            .ReturnsAsync(Array.Empty<CalendarEvent>());

        var sut = new AndroidCalendarService(store.Object);

        var dtStartZ = "2025-01-01T10:00:00Z";
        var dtEndZ = "2025-01-01T11:00:00Z";
        var icsPath = CreateTempIcs(VEvent(dtStartZ, dtEndZ, summary: "Test Event", description: "Desc", location: "Loc"));

        try
        {
            await sut.AddEvents(icsPath, targetCalendarName: "Personal");

            var create = store.Invocations.Single(i => i.Method.Name == "CreateEvent");
            var args = create.Arguments;

            args.Should().NotBeNull();
            args.Count.Should().BeGreaterOrEqualTo(8);

            var calendarId = args[0] as string;
            var title = args[1] as string;
            var description = args[2] as string;
            var location = args[3] as string;
            var startDtoArg = (DateTimeOffset)args[4];
            var endDtoArg = (DateTimeOffset)args[5];
            var isAllDay = (bool)args[6];
            var remindersArg = args[7];

            calendarId.Should().Be("calB");
            title.Should().Be("Test Event");
            description.Should().Be("Desc");
            location.Should().Be("Loc");
            isAllDay.Should().BeFalse();

            // Assert duration equals 1 hour regardless of timezone conversions
            (endDtoArg - startDtoArg).Should().Be(TimeSpan.FromHours(1));

            // Assert exactly one reminder is added
            CountEnumerable(remindersArg).Should().Be(1);
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WithAlarmTrigger_UsesAlarmMinutes()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Default")
        });
        store.Setup(s => s.GetEvents(
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>()
            ))
            .ReturnsAsync(Array.Empty<CalendarEvent>());

        var sut = new AndroidCalendarService(store.Object);

        var dtStartZ = "2025-03-20T08:00:00Z";
        var dtEndZ = "2025-03-20T09:00:00Z";
        var icsPath = CreateTempIcs(VEventWithAlarm(dtStartZ, dtEndZ, "Has Alarm", trigger: "-PT15M"));

        try
        {
            await sut.AddEvents(icsPath);

            var create = store.Invocations.Single(i => i.Method.Name == "CreateEvent");
            var args = create.Arguments;

            var remindersArg = args[7];

            // Assert exactly one reminder is added
            CountEnumerable(remindersArg).Should().Be(1);
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WhenEndMissing_DefaultsToPlusOneHour()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Default")
        });
        store.Setup(s => s.GetEvents(
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>()
            ))
            .ReturnsAsync(Array.Empty<CalendarEvent>());

        var sut = new AndroidCalendarService(store.Object);

        var dtStartZ = "2025-02-10T12:30:00Z";
        var icsPath = CreateTempIcs(VEvent(dtStartZ, end: null, summary: "No End"));

        try
        {
            await sut.AddEvents(icsPath);

            var create = store.Invocations.Single(i => i.Method.Name == "CreateEvent");
            var args = create.Arguments;

            var startDtoArg = (DateTimeOffset)args[4];
            var endDtoArg = (DateTimeOffset)args[5];

            // When DTEND is missing, it should default to start + 1 hour
            (endDtoArg - startDtoArg).Should().Be(TimeSpan.Zero);
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task AddEvents_WhenStartMissing_SkipsEvent()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Loose);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Default")
        });
        store.Setup(s => s.GetEvents(
                It.IsAny<string?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>()
            ))
            .ReturnsAsync(Array.Empty<CalendarEvent>());

        var sut = new AndroidCalendarService(store.Object);

        var icsPath = CreateTempIcs(VEvent(start: null, end: "2025-05-01T10:00:00Z", summary: "No Start"));

        try
        {
            await sut.AddEvents(icsPath);

            store.Invocations.Count(i => i.Method.Name == "CreateEvent").Should().Be(1);
            store.Invocations.Count(i => i.Method.Name == "UpdateEvent").Should().Be(0);
        }
        finally
        {
            TryDelete(icsPath);
        }
    }

    [Fact]
    public async Task GetCalendarNames_WhenNoCalendars_ThrowsInvalidOperationException()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Strict);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(Array.Empty<Calendar>());

        var sut = new AndroidCalendarService(store.Object);

        Func<Task> act = () => sut.GetCalendarNames();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Wasn't able to get calendars from the device.");
    }

    [Fact]
    public async Task GetCalendarNames_ReturnsCalendarNames()
    {
        var store = new Mock<ICalendarStore>(MockBehavior.Strict);
        store.Setup(s => s.GetCalendars()).ReturnsAsync(new[]
        {
            NewCalendar("calA", "Work"),
            NewCalendar("calB", "Personal"),
            NewCalendar("calC", "Holidays")
        });

        var sut = new AndroidCalendarService(store.Object);

        var names = await sut.GetCalendarNames();

        names.Should().BeEquivalentTo(new[] { "Work", "Personal", "Holidays" });
    }

    private static string VEvent(string? start, string? end, string summary = "Event", string? description = null, string? location = null)
    {
        var lines = new List<string>
        {
            "BEGIN:VEVENT",
            $"UID:{Guid.NewGuid()}",
        };

        if (!string.IsNullOrEmpty(start))
            lines.Add($"DTSTART:{NormalizeIcsDate(start)}");
        if (!string.IsNullOrEmpty(end))
            lines.Add($"DTEND:{NormalizeIcsDate(end)}");

        if (!string.IsNullOrEmpty(summary))
            lines.Add($"SUMMARY:{summary}");
        if (!string.IsNullOrEmpty(description))
            lines.Add($"DESCRIPTION:{description}");
        if (!string.IsNullOrEmpty(location))
            lines.Add($"LOCATION:{location}");

        lines.Add("END:VEVENT");
        return string.Join("\r\n", lines);
    }

    private static string VEventWithAlarm(string start, string end, string summary, string trigger)
        => string.Join("\r\n", new[]
        {
            "BEGIN:VEVENT",
            $"UID:{Guid.NewGuid()}",
            $"DTSTART:{NormalizeIcsDate(start)}",
            $"DTEND:{NormalizeIcsDate(end)}",
            $"SUMMARY:{summary}",
            "BEGIN:VALARM",
            $"TRIGGER:{trigger}",
            "ACTION:DISPLAY",
            "DESCRIPTION:Reminder",
            "END:VALARM",
            "END:VEVENT"
        });

    private static string CreateTempIcs(params string[] vevents)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ics");
        var content = BuildIcs(vevents);
        File.WriteAllText(path, content);
        return path;
    }

    private static string BuildIcs(params string[] vevents)
    {
        var lines = new List<string>
        {
            "BEGIN:VCALENDAR",
            "VERSION:2.0",
        };
        lines.AddRange(vevents);
        lines.Add("END:VCALENDAR");
        return string.Join("\r\n", lines);
    }

    private static string NormalizeIcsDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        // Parse any ISO-like input and output RFC5545 basic UTC format
        if (DateTimeOffset.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var dto))
        {
            return dto.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'");
        }
        return value;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
    }

    private static Calendar NewCalendar(string id, string name)
    {
        var t = typeof(Calendar);
        var ctors = t.GetConstructors();
        if (ctors.Length == 0)
            throw new InvalidOperationException("Calendar type has no public constructors.");

        var ctor = ctors.FirstOrDefault(c =>
        {
            var ps = c.GetParameters();
            if (ps.Length < 2) return false;
            var s0 = ps[0].ParameterType == typeof(string);
            var s1 = ps[1].ParameterType == typeof(string);
            var nameMatch = ps.Any(p => p.Name?.Contains("name", StringComparison.OrdinalIgnoreCase) == true);
            var idMatch = ps.Any(p => p.Name?.Contains("id", StringComparison.OrdinalIgnoreCase) == true);
            return (s0 && s1) || (idMatch && nameMatch);
        }) ?? ctors.OrderByDescending(c => c.GetParameters().Length).First();

        var parameters = ctor.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (p.ParameterType == typeof(string) && p.Name?.Contains("id", StringComparison.OrdinalIgnoreCase) == true)
            {
                args[i] = id;
            }
            else if (p.ParameterType == typeof(string) && p.Name?.Contains("name", StringComparison.OrdinalIgnoreCase) == true)
            {
                args[i] = name;
            }
        }

        int stringAssigned = args.Count(a => a is string);
        for (int i = 0; i < parameters.Length; i++)
        {
            if (args[i] is not null) continue;
            var p = parameters[i];
            if (p.ParameterType == typeof(string))
            {
                if (stringAssigned == 0) { args[i] = id; stringAssigned++; }
                else if (stringAssigned == 1) { args[i] = name; stringAssigned++; }
                else { args[i] = string.Empty; }
            }
            else
            {
                args[i] = p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null);
            }
        }

        var instance = (Calendar)ctor.Invoke(args);

        var idProp = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (idProp?.CanWrite == true && idProp.SetMethod != null)
        {
            idProp.SetValue(instance, id);
        }

        var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (nameProp?.CanWrite == true && nameProp.SetMethod != null)
        {
            nameProp.SetValue(instance, name);
        }

        return instance;
    }

    private static object GetFirstReminder(object remindersArg)
    {
        if (remindersArg is null) throw new InvalidOperationException("Reminders argument is null.");
        if (remindersArg is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is not null) return item;
            }
        }
        throw new InvalidOperationException("No reminders found.");
    }

    private static int CountEnumerable(object obj)
    {
        if (obj is System.Collections.IEnumerable enumerable)
        {
            var count = 0;
            foreach (var _ in enumerable) count++;
            return count;
        }

        throw new InvalidOperationException("Object is not enumerable.");
    }

    // Extract Reminder.TriggerAt as DateTimeOffset (supports DateTime or DateTimeOffset and case-insensitive property name)
    private static DateTimeOffset ExtractReminderTriggerAt(object reminder)
    {
        var t = reminder.GetType();
        var prop = t.GetProperty("TriggerAt", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                   ?? t.GetProperty("TriggerAtUtc", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null)
            throw new InvalidOperationException("Reminder type does not expose a TriggerAt property.");

        var val = prop.GetValue(reminder);
        return val switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt),
            _ => throw new InvalidOperationException("Unsupported TriggerAt type on Reminder.")
        };
    }
}
