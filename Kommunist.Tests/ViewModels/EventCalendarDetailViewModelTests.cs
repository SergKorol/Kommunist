using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Kommunist.Application.Models;
using Kommunist.Application.ViewModels;
using Kommunist.Core.ApiModels;
using Kommunist.Core.ApiModels.PageProperties.Agenda;
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
        eventService.Setup(s => s.GetAgenda(It.IsAny<int>()))
            .ReturnsAsync((AgendaPage?)null);

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

        vm.PropertyChanged += Handler;

        using var cts = new CancellationTokenSource(timeout.Value);
        await using var _ = cts.Token.Register(() =>
        {
            vm.PropertyChanged -= Handler;
            tcs.TrySetException(new TimeoutException("Timed out waiting for SelectedEventDetail to be set."));
        });

        return await tcs.Task.ConfigureAwait(false);

        void Handler(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(EventCalendarDetailViewModel.SelectedEventDetail) &&
                !string.IsNullOrEmpty(args.PropertyName)) return;
            if (vm.SelectedEventDetail is null) return;
            vm.PropertyChanged -= Handler;
            tcs.TrySetResult(vm.SelectedEventDetail);
        }
    }

    private static T CallPrivateStatic<T>(string methodName, Type[]? parameterTypes, params object?[] args)
    {
        var type = typeof(EventCalendarDetailViewModel);
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

        var method = parameterTypes is null ? type.GetMethod(methodName, flags) : type.GetMethod(methodName, flags, binder: null, types: parameterTypes, modifiers: null);

        Assert.NotNull(method);
        var result = method.Invoke(null, args);

        return result switch
        {
            null => throw new Xunit.Sdk.XunitException(
                $"Invoked method '{methodName}' returned null, expected a value of type '{typeof(T)}'."),
            T typed => typed,
            _ => throw new Xunit.Sdk.XunitException(
                $"Invoked method '{methodName}' returned type '{result.GetType()}' which is not assignable to expected type '{typeof(T)}'.")
        };
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
                    ParticipationFormat = null
                },
                Languages = ["en", "ru"],
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

        var vm = CreateVmWithPageItems([main, unlimitedText]);

        // Act
        var detail = await WaitForSelectedEventDetailAsync(vm);

        // Assert
        Assert.NotNull(detail);
        Assert.Equal("https://img.example.com/bg.jpg", detail.BgImageUrl);

        Assert.Equal("EN, RU", detail.Language);

        Assert.Equal("Offline", detail.FormatEvent);

        Assert.Equal("World", detail.Location);

        Assert.False(string.IsNullOrWhiteSpace(detail.Description));
        Assert.Contains("Hello world", detail.Description);
        Assert.Contains("<html>", detail.Description, StringComparison.OrdinalIgnoreCase);

        Assert.Equal("https://event.example.com", detail.Url);

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

        Assert.False(vm.HasParticipants);

        detail.Speakers.Add(new PersonCard { Name = "Alice" });
        Assert.True(vm.HasParticipants);

        detail.Speakers.Clear();
        Assert.False(vm.HasParticipants);

        detail.Moderators.Add(new PersonCard { Name = "Bob" });
        Assert.True(vm.HasParticipants);
    }

    [Fact]
    public void MakeSafeFileName_Replaces_Invalid_Chars_And_Trims()
    {
        // Arrange
        const string input = "  in:va*lid?name  ";

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

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // Act
            var result = CallPrivateStatic<string>(
                "GetEventPeriod",
                [typeof(long?), typeof(long?)],
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
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void GetEventPeriod_Formats_Multi_Day_Correctly()
    {
        // Arrange
        var start = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var end = new DateTimeOffset(2024, 1, 2, 9, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // Act
            var result = CallPrivateStatic<string>(
                "GetEventPeriod",
                [typeof(long?), typeof(long?)],
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
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task JoinToEvent_With_Missing_Url_Does_Not_Throw()
    {
        // Arrange
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

        var vm = CreateVmWithPageItems([main]);
        _ = await WaitForSelectedEventDetailAsync(vm);

        // Act + Assert
        var ex = await Record.ExceptionAsync(async () =>
        {
            vm.JoinToEvent.Execute(null);
            await Task.Delay(100);
        });

        Assert.Null(ex);
    }

    [Fact]
    public async Task IsWebViewLoading_Raises_PropertyChanged()
    {
        // Arrange
        var eventService = new Mock<IEventService>(MockBehavior.Strict);
        var fileHosting = new Mock<IFileHostingService>(MockBehavior.Loose);
        var androidCalendar = new Mock<IAndroidCalendarService>(MockBehavior.Loose);

        var vm = new EventCalendarDetailViewModel(eventService.Object, 0, fileHosting.Object, androidCalendar.Object);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EventCalendarDetailViewModel.IsWebViewLoading))
                tcs.TrySetResult(true);
        };

        // Act
        vm.IsWebViewLoading = !vm.IsWebViewLoading;

        // Assert
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(1))) == tcs.Task;
        Assert.True(completed);
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
