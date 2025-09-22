using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Kommunist.Application.Models;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Moq;

namespace Kommunist.Tests.ViewModels;

public class EventCalendarDetailViewModelTests
{
    private static EventCalendarDetailViewModel CreateVmWithPageItems(
        IEnumerable<PageItem> pageItems,
        int eventId = 123)
    {
        var eventService = new Mock<IEventService>(MockBehavior.Strict);
        eventService.Setup(s => s.GetHomePage(eventId))
            .ReturnsAsync(pageItems);
        // Agenda not needed for these tests; keep default behavior safe
        eventService.Setup(s => s.GetAgenda(It.IsAny<int>()))
            .ReturnsAsync((Kommunist.Core.Entities.PageProperties.Agenda.AgendaPage?)null);

        var fileHosting = new Mock<IFileHostingService>(MockBehavior.Loose);
        var androidCalendar = new Mock<IAndroidCalendarService>(MockBehavior.Loose);

        return new EventCalendarDetailViewModel(eventService.Object, eventId, fileHosting.Object, androidCalendar.Object);
    }

    private static async Task<CalEventDetail> WaitForSelectedEventDetailAsync(
        EventCalendarDetailViewModel vm,
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);

        if (vm.SelectedEventDetail is not null)
            return vm.SelectedEventDetail;

        var tcs = new TaskCompletionSource<CalEventDetail>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Handler(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(EventCalendarDetailViewModel.SelectedEventDetail) ||
                string.IsNullOrEmpty(args.PropertyName))
            {
                if (vm.SelectedEventDetail is not null)
                {
                    vm.PropertyChanged -= Handler;
                    tcs.TrySetResult(vm.SelectedEventDetail);
                }
            }
        }

        vm.PropertyChanged += Handler;

        using var cts = new CancellationTokenSource(timeout.Value);
        await using var _ = cts.Token.Register(() =>
        {
            vm.PropertyChanged -= Handler;
            tcs.TrySetException(new TimeoutException("Timed out waiting for SelectedEventDetail to be set."));
        });

        return await tcs.Task.ConfigureAwait(false);
    }

    private static T CallPrivateStatic<T>(string methodName, Type[]? parameterTypes, params object?[] args)
    {
        var type = typeof(EventCalendarDetailViewModel);
        var flags = BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo? method;

        method = parameterTypes is null ? type.GetMethod(methodName, flags) : type.GetMethod(methodName, flags, binder: null, types: parameterTypes, modifiers: null);

        Assert.NotNull(method);
        var result = method.Invoke(null, args);
        return (T)result!;
    }

    [Fact]
    public async Task CreateEvent_Populates_SelectedEventDetail_From_Main_And_UnlimitedText()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 01, 01, 10, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var end = new DateTimeOffset(2024, 01, 01, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        var main = new PageItem
        {
            Type = "Main",
            Properties = new Properties
            {
                Image = new ImageDetails { Url = "https://img.example.com/bg.jpg" },
                Details = new Details
                {
                    DatesTimestamp = new DatesTimestamp { Start = start, End = end },
                    ParticipationFormat = null // triggers Offline + World
                },
                Languages = new List<string> { "en", "ru" },
                EventUrl = "https://event.example.com"
            }
        };

        var unlimitedText = new PageItem
        {
            Type = "UnlimitedText",
            Properties = new Properties
            {
                UnlimitedText = "<p>Hello world</p>"
            }
        };

        var vm = CreateVmWithPageItems(new[] { main, unlimitedText });

        // Act
        var detail = await WaitForSelectedEventDetailAsync(vm);

        // Assert
        Assert.NotNull(detail);
        Assert.Equal("https://img.example.com/bg.jpg", detail.BgImageUrl);

        // Language should be upper-cased and comma-separated
        Assert.Equal("EN, RU", detail.Language);

        // Online/Offline should default to Offline when ParticipationFormat is null
        Assert.Equal("Offline", detail.FormatEvent);

        // Location should fallback to World when ParticipationFormat is null
        Assert.Equal("World", detail.Location);

        // Description should be HTML-wrapped (light mode by default)
        Assert.False(string.IsNullOrWhiteSpace(detail.Description));
        Assert.Contains("Hello world", detail.Description);
        Assert.Contains("<html>", detail.Description, StringComparison.OrdinalIgnoreCase);

        // Event URL mapped
        Assert.Equal("https://event.example.com", detail.Url);

        // Period string present
        Assert.False(string.IsNullOrWhiteSpace(detail.PeriodDateTime));
    }

    [Fact]
    public async Task HasParticipants_Computed_From_Speakers_And_Moderators()
    {
        // Arrange
        var pageItems = new[]
        {
            new PageItem
            {
                Type = "Main",
                Properties = new Properties
                {
                    Details = new Details
                    {
                        DatesTimestamp = new DatesTimestamp { Start = 1704067200, End = 1704074400 }
                    }
                }
            }
        };

        var vm = CreateVmWithPageItems(pageItems);
        var detail = await WaitForSelectedEventDetailAsync(vm);

        // Initially false
        Assert.False(vm.HasParticipants);

        // Add a speaker
        detail.Speakers.Add(new PersonCard { Name = "Alice" });
        Assert.True(vm.HasParticipants);

        // Clear and add a moderator
        detail.Speakers.Clear();
        Assert.False(vm.HasParticipants);

        detail.Moderators.Add(new PersonCard { Name = "Bob" });
        Assert.True(vm.HasParticipants);
    }

    [Fact]
    public void MakeSafeFileName_Replaces_Invalid_Chars_And_Trims()
    {
        // Arrange
        var input = "  in:va*lid?name  ";

        // Act
        var result = CallPrivateStatic<string>(
            "MakeSafeFileName",
            [typeof(string)],
            input
        );

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Equal(result, result.Trim()); // trimmed
        AssertExtensions.DoesNotContainAny(result, Path.GetInvalidFileNameChars());
    }

    [Fact]
    public void BuildIcsDescription_Works_For_Empty_Inputs()
    {
        // Act
        var result = CallPrivateStatic<string>(
            "BuildIcsDescription",
            [typeof(string), typeof(string)],
            null, null
        );

        // Assert
        Assert.Equal(string.Empty, result);
    }
    

    [Fact]
    public void ConvertDateTime_Returns_Local_DateTime()
    {
        // Arrange
        var unix = new DateTimeOffset(2024, 1, 2, 15, 30, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        // Act
        var result = CallPrivateStatic<DateTime>(
            "ConvertDateTime",
            [typeof(long)],
            unix
        );

        // Assert
        var expected = DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
        Assert.Equal(expected, result);
        Assert.Equal(DateTimeKind.Local, result.Kind);
    }

    [Fact]
    public void GetEventPeriod_Formats_Same_Day_Correctly()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var end = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        // Force deterministic culture
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // Act
            var result = CallPrivateStatic<string>(
                "GetEventPeriod",
                new[] { typeof(long?), typeof(long?) },
                (long?)start, (long?)end
            );

            // Assert
            var s = DateTimeOffset.FromUnixTimeSeconds(start).LocalDateTime;
            var e = DateTimeOffset.FromUnixTimeSeconds(end).LocalDateTime;
            var expected = $"{s:d MMM yyyy, HH:mm}-{e:HH:mm}";
            Assert.Equal(expected, result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }

    [Fact]
    public void GetEventPeriod_Formats_Multi_Day_Correctly()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var end = new DateTimeOffset(2024, 1, 2, 9, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // Act
            var result = CallPrivateStatic<string>(
                "GetEventPeriod",
                new[] { typeof(long?), typeof(long?) },
                (long?)start, (long?)end
            );

            // Assert
            var s = DateTimeOffset.FromUnixTimeSeconds(start).LocalDateTime;
            var e = DateTimeOffset.FromUnixTimeSeconds(end).LocalDateTime;
            var expected = $"{s:d MMM yyyy, HH:mm} - {e:d MMM yyyy, HH:mm}";
            Assert.Equal(expected, result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
        }
    }

    [Fact]
    public async Task JoinToEvent_With_Missing_Url_Does_Not_Throw()
    {
        // Arrange: Provide only Main item without EventUrl
        var main = new PageItem
        {
            Type = "Main",
            Properties = new Properties
            {
                Details = new Details
                {
                    DatesTimestamp = new DatesTimestamp { Start = 1704067200, End = 1704070800 }
                }
            }
        };

        var vm = CreateVmWithPageItems(new[] { main });
        var _ = await WaitForSelectedEventDetailAsync(vm);

        // Act + Assert: command should execute without throwing, despite internal Toast/Launcher usage
        var ex = await Record.ExceptionAsync(async () =>
        {
            // Command is async void; execute and wait a tiny delay to allow inner await to run
            vm.JoinToEvent.Execute(null);
            await Task.Delay(100);
        });

        Assert.Null(ex);
    }

    [Fact]
    public void IsWebViewLoading_Raises_PropertyChanged()
    {
        // Arrange
        var eventService = new Mock<IEventService>(MockBehavior.Strict);
        var fileHosting = new Mock<IFileHostingService>(MockBehavior.Loose);
        var androidCalendar = new Mock<IAndroidCalendarService>(MockBehavior.Loose);

        // Use eventId=0 to avoid background loading during this test
        var vm = new EventCalendarDetailViewModel(eventService.Object, 0, fileHosting.Object, androidCalendar.Object);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(EventCalendarDetailViewModel.IsWebViewLoading))
                tcs.TrySetResult(true);
        };

        // Act
        vm.IsWebViewLoading = !vm.IsWebViewLoading;

        // Assert
        Assert.True(tcs.Task.Wait(TimeSpan.FromSeconds(1)));
    }
}

internal static class AssertExtensions
{
    public static void DoesNotContainAny(string value, IEnumerable<char> anyOf)
    {
        foreach (var ch in anyOf)
        {
            if (value.Contains(ch))
                throw new Xunit.Sdk.XunitException($"The string should not contain invalid character '{ch}'. Actual: \"{value}\"");
        }
    }
}
