using FluentAssertions;
using Kommunist.Application.Models;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Entities;
using Kommunist.Core.Services.Interfaces;
using Moq;
using System.Reflection;
using XCalendar.Core.Models;

namespace Kommunist.Tests.ViewModels;

public class EventCalendarViewModelTests
{
    private static EventCalendarViewModel CreateViewModel(
        out Mock<IEventService> eventServiceMock,
        IEventService? eventService = null)
    {
        eventServiceMock = new Mock<IEventService>(MockBehavior.Strict);

        var es = eventService ?? eventServiceMock.Object;
        var fileHosting = Mock.Of<Kommunist.Core.Services.Interfaces.IFileHostingService>();
        var androidCalendar = Mock.Of<Kommunist.Core.Services.Interfaces.IAndroidCalendarService>();

        return new EventCalendarViewModel(es, fileHosting, androidCalendar);
    }

    private static void SetCalendarNavigatedDate(Calendar<EventDay> calendar, DateTime target)
    {
        var delta = target - calendar.NavigatedDate;
        calendar.Navigate(delta);
    }

    // Detach the VM's internal DaysUpdated handler to prevent it from overwriting CalEvents during tests
    private static void DetachDaysUpdated(EventCalendarViewModel vm)
    {
        var method = typeof(EventCalendarViewModel).GetMethod("EventCalendar_DaysUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != null)
        {
            var handler = (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), vm, method);
            vm.EventCalendar.DaysUpdated -= handler;
        }
    }

    [Fact]
    public async Task RefreshCalendarEvents_CallsServiceWithMonthRange_AndRaisesPropertyChanged()
    {
        // Arrange
        var eventServiceMock = new Mock<IEventService>(MockBehavior.Strict);
        eventServiceMock
            .Setup(s => s.LoadEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Empty<ServiceEvent>());

        var vm = CreateViewModel(out _, eventServiceMock.Object);

        // Prevent DaysUpdated from triggering a service call during navigation
        DetachDaysUpdated(vm);

        // Navigate calendar to a specific month to control date range (leap year to check boundaries)
        var navigated = new DateTime(2024, 2, 15);
        SetCalendarNavigatedDate(vm.EventCalendar, navigated);

        var propertyChanges = new List<string>();
        vm.PropertyChanged += (_, e) => propertyChanges.Add(e.PropertyName ?? string.Empty);

        var expectedStart = new DateTime(2024, 2, 1);
        var expectedEnd = new DateTime(2024, 3, 1).AddTicks(-1);

        // Act
        await vm.RefreshCalendarEvents();

        // Assert
        eventServiceMock.Verify(s => s.LoadEvents(
                It.Is<DateTime>(d => d == expectedStart),
                It.Is<DateTime>(d => d == expectedEnd)),
            Times.Once);

        // PropertyChanged raised for both collections
        propertyChanges.Should().Contain(nameof(vm.CalEvents));
        propertyChanges.Should().Contain(nameof(vm.SelectedEvents));

        // No events returned => both collections should be empty
        vm.CalEvents.Should().BeEmpty();
        vm.SelectedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeDateSelectionCommand_UpdatesSelectedEvents_ForSelectedDate()
    {
        // Arrange
        var vm = CreateViewModel(out var eventServiceMock);
        eventServiceMock
            .Setup(s => s.LoadEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Empty<ServiceEvent>());

        // Prevent DaysUpdated from overwriting CalEvents
        DetachDaysUpdated(vm);

        var nav = vm.EventCalendar.NavigatedDate;
        var date1 = new DateTime(nav.Year, nav.Month, 10, 9, 0, 0);
        var date2 = new DateTime(nav.Year, nav.Month, 11, 10, 0, 0);

        vm.CalEvents.ReplaceRange(new[]
        {
            new CalEvent { EventId = 1, Title = "A", DateTime = date1 },
            new CalEvent { EventId = 2, Title = "B", DateTime = date2 },
            new CalEvent { EventId = 3, Title = "C", DateTime = date1.AddHours(1) }
        });

        // Act
        vm.EventCalendar.SelectedDates.Add(date1.Date);

        // Assert
        vm.SelectedEvents.Should().HaveCount(2);
        vm.SelectedEvents.Select(e => e.EventId).Should().BeEquivalentTo(new[] { 1, 3 }, opts => opts.WithoutStrictOrdering());
    }

    [Fact]
    public void ChangeDateSelectionCommand_TogglesSelection_AndClearsSelectedEvents()
    {
        // Arrange
        var vm = CreateViewModel(out var eventServiceMock);
        eventServiceMock
            .Setup(s => s.LoadEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Empty<ServiceEvent>());

        // Prevent DaysUpdated from overwriting CalEvents
        DetachDaysUpdated(vm);

        var nav = vm.EventCalendar.NavigatedDate;
        var date = new DateTime(nav.Year, nav.Month, 3, 12, 0, 0);

        vm.CalEvents.ReplaceRange(new[]
        {
            new CalEvent { EventId = 1, Title = "A", DateTime = date },
            new CalEvent { EventId = 2, Title = "B", DateTime = date.AddHours(2) }
        });

        // Select
        vm.EventCalendar.SelectedDates.Add(date.Date);
        vm.SelectedEvents.Should().HaveCount(2);

        // Toggle off (same date) - simulate by removing from SelectedDates
        vm.EventCalendar.SelectedDates.Remove(date.Date);

        // Assert
        vm.SelectedEvents.Should().BeEmpty();
    }

    [Fact]
    public void SelectedEvents_AreSortedByDateTimeDescending()
    {
        // Arrange
        var vm = CreateViewModel(out var eventServiceMock);
        eventServiceMock
            .Setup(s => s.LoadEvents(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(Enumerable.Empty<ServiceEvent>());

        // Prevent DaysUpdated from overwriting CalEvents
        DetachDaysUpdated(vm);

        var nav = vm.EventCalendar.NavigatedDate;
        var day = new DateTime(nav.Year, nav.Month, 20);
        var e1 = new CalEvent { EventId = 1, Title = "Morning", DateTime = day.AddHours(9) };
        var e2 = new CalEvent { EventId = 2, Title = "Afternoon", DateTime = day.AddHours(13) };
        var e3 = new CalEvent { EventId = 3, Title = "Evening", DateTime = day.AddHours(18) };

        vm.CalEvents.ReplaceRange(new[] { e1, e2, e3 });

        // Act
        vm.EventCalendar.SelectedDates.Add(day.Date);

        // Assert
        vm.SelectedEvents.Select(x => x.EventId).Should().Equal(3, 2, 1);
    }

    [Fact]
    public void NavigateCalendarCommand_MovesMonth_ForwardsAndBackwards()
    {
        // Arrange
        var vm = CreateViewModel(out var _);

        var initial = new DateTime(2025, 4, 15);
        SetCalendarNavigatedDate(vm.EventCalendar, initial);

        // Act - forward one month
        vm.NavigateCalendarCommand.Execute(1);

        // Assert
        vm.EventCalendar.NavigatedDate.Year.Should().Be(2025);
        vm.EventCalendar.NavigatedDate.Month.Should().Be(5);

        // Act - backwards two months from May -> March
        vm.NavigateCalendarCommand.Execute(-2);

        // Assert
        vm.EventCalendar.NavigatedDate.Year.Should().Be(2025);
        vm.EventCalendar.NavigatedDate.Month.Should().Be(3);
    }
}