using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Application.Views;
using Kommunist.Core.ApiModels;
using Kommunist.Core.Services.Interfaces;
using XCalendar.Core.Collections;
using XCalendar.Core.Enums;
using XCalendar.Core.Extensions;
using XCalendar.Core.Models;

namespace Kommunist.Application.ViewModels;

public class EventCalendarViewModel : BaseViewModel
{
    #region Properties
    public Calendar<EventDay> EventCalendar { get; } = new()
    {
        SelectedDates = [],
        SelectionType = SelectionType.Single
    };

    private static readonly Random Random = Random.Shared;

    private static readonly IReadOnlyList<Color> Colors =
    [
        Microsoft.Maui.Graphics.Colors.Red,
        Microsoft.Maui.Graphics.Colors.Orange,
        Microsoft.Maui.Graphics.Colors.Yellow,
        Color.FromArgb("#00A000"),
        Microsoft.Maui.Graphics.Colors.Blue,
        Color.FromArgb("#8010E0")
    ];

    public ObservableRangeCollection<CalEvent> CalEvents { get; } = [];
    public ObservableRangeCollection<CalEvent> SelectedEvents { get; } = [];
    #endregion

    #region Commands
    public ICommand NavigateCalendarCommand { get; set; }
    public ICommand ChangeDateSelectionCommand { get; set; }
    public ICommand EventSelectedCommand { get; }
    public ICommand OpenIcalConfigCommand { get; }
    #endregion

    #region Services
    private readonly IEventService _eventService;
    private readonly IFileHostingService _fileHostingService;
    private readonly IAndroidCalendarService _androidCalendarService;
    #endregion

    #region Fields
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private readonly Dictionary<string, List<CalEvent>> _monthEventsCache = new();
    #endregion
    
    #region Constructors
    public EventCalendarViewModel(IEventService eventService, IFileHostingService fileHostingService, IAndroidCalendarService androidCalendarService)
    {
        _eventService = eventService;
        _fileHostingService = fileHostingService;
        _androidCalendarService = androidCalendarService;
        NavigateCalendarCommand = new Command<int>(NavigateCalendar);
        ChangeDateSelectionCommand = new Command<DateTime>(ChangeDateSelection);
        EventSelectedCommand = new Command<CalEvent>(OnEventSelected);

        EventCalendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;
        EventCalendar.DaysUpdated += EventCalendar_DaysUpdated;

        OpenIcalConfigCommand = new Command(OpenIcalConfig);
    }
    #endregion

    #region Public
    public async Task RefreshCalendarEvents()
    {
        try
        {
            var monthKey = GetMonthKey(EventCalendar.NavigatedDate);
            _monthEventsCache.Remove(monthKey);

            await LoadAndPopulateEventsAsync();

            _monthEventsCache[monthKey] = CalEvents.ToList();

            UpdateDaysWithCalEvents();

            if (EventCalendar.SelectedDates.Any())
            {
                UpdateSelectedEvents();
            }

            OnPropertyChanged(nameof(CalEvents));
            OnPropertyChanged(nameof(SelectedEvents));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing calendar events: {ex.Message}");
            await Toast.Make("Failed to refresh events. Please try again.").Show();
        }
    }
    #endregion
    
    #region Private
    private async void EventCalendar_DaysUpdated(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Calendar<EventDay> calendar) return;

            var monthKey = GetMonthKey(calendar.NavigatedDate);

            if (_monthEventsCache.TryGetValue(monthKey, out var cached))
            {
                CalEvents.ReplaceRange(cached);
            }
            else
            {
                await LoadAndPopulateEventsAsync();
                _monthEventsCache[monthKey] = CalEvents.ToList();
            }

            UpdateDaysWithCalEvents();
        }
        catch (Exception ex)
        {
            await Toast.Make($"Failed to update calendar: {ex.Message}").Show();
        }
    }

    private void SelectedDates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateSelectedEvents();
    }

    private void NavigateCalendar(int amount)
    {
        if (EventCalendar.NavigatedDate.TryAddMonths(amount, out var targetDate))
        {
            EventCalendar.Navigate(targetDate - EventCalendar.NavigatedDate);
        }
        else
        {
            EventCalendar.Navigate(amount > 0 ? TimeSpan.MaxValue : TimeSpan.MinValue);
        }
    }

    private void ChangeDateSelection(DateTime dateTime)
    {
        EventCalendar.ChangeDateSelection(dateTime);
    }

    private async void OnEventSelected(CalEvent selectedEvent)
    {
        try
        {
            var eventDetailViewModel = new EventCalendarDetailViewModel(_eventService, selectedEvent.EventId, _fileHostingService, _androidCalendarService);
            await Shell.Current.Navigation.PushAsync(new CalEventDetailPage(eventDetailViewModel));
        }
        catch (Exception e)
        {
            await Toast.Make($"Failed to load event detail: {e.Message}").Show();
        }
    }

    private async void OpenIcalConfig()
    {
        try
        {
            if (!SelectedEvents.Any())
            {
                await Toast.Make("Please select at least one event.").Show();
                return;
            }

            var navigationParams = new Dictionary<string, object>
            {
                { "SelectedEvents", SelectedEvents.ToList() }
            };
            await Shell.Current.GoToAsync("//ICalConfigPage", navigationParams);
        }
        catch (Exception e)
        {
            await Toast.Make($"Failed to load iCal config: {e.Message}").Show();
        }
    }
    
    private async Task LoadAndPopulateEventsAsync()
    {
        await _loadSemaphore.WaitAsync();
        try
        {
            var dateRange = GetDateRangeForNavigatedMonth();
            var serviceEvents = await _eventService.LoadEvents(dateRange.StartDate, dateRange.EndDate);
            var calEvents = ConvertToCalEvents(serviceEvents);
            CalEvents.ReplaceRange(calEvents);
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }
    
    private void UpdateDaysWithCalEvents()
    {
        foreach (var day in EventCalendar.Days)
        {
            day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
        }
    }
    
    private void UpdateSelectedEvents()
    {
        SelectedEvents.ReplaceRange(
            CalEvents
                .Where(x => EventCalendar.SelectedDates.Any(y => x.DateTime.Date == y.Date))
                .OrderByDescending(x => x.DateTime));
    }
    
    private (DateTime StartDate, DateTime EndDate) GetDateRangeForNavigatedMonth()
    {
        var nav = EventCalendar.NavigatedDate.Date;
        var start = new DateTime(nav.Year, nav.Month, 1);
        var end = start.AddMonths(1).AddTicks(-1);
        return (start, end);
    }
    #endregion
    
    #region Static
    private static List<CalEvent> ConvertToCalEvents(IEnumerable<ServiceEvent>? serviceEvents)
    {
        if (serviceEvents != null)
            return serviceEvents.Select(e => new CalEvent
            {
                EventId = e.Id,
                Title = e.Title,
                Location = e.ParticipationFormat is { Online: true } ? string.Empty : e.ParticipationFormat?.Location,
                Description = BuildEventDescription(e),
                DateTime = e.Start.ToLocalDateTime(),
                Start = e.Start,
                End = e.End,
                Color = Colors[Random.Next(Colors.Count)],
                Url = $"https://wearecommunity.io/events/{e.EventUrl}"
            }).ToList();
        
        return [];
    }

    private static string BuildEventDescription(ServiceEvent serviceEvent)
    {
        var startLocal = serviceEvent.Start.ToLocalDateTime();
        var endLocal = serviceEvent.End.ToLocalDateTime();

        var datePart = startLocal.Date == endLocal.Date
            ? startLocal.ToString("dd.MM.yyyy")
            : $"{startLocal:dd.MM.yyyy} - {endLocal:dd.MM.yyyy}";

        var languages = serviceEvent.Languages is { Count: > 0 }
            ? string.Join("/", serviceEvent.Languages.ReplaceCodesWithFlags())
            : "N/A";

        var location = serviceEvent.ParticipationFormat is { Online: true } ? string.Empty : serviceEvent.ParticipationFormat?.Location;

        var baseDescription = $"{datePart}, {languages}";

        return string.IsNullOrWhiteSpace(location)
            ? baseDescription
            : $"{baseDescription}, {location}";
    }

    private static string GetMonthKey(DateTime date) => $"{date:yyyy-MM}";
    #endregion
}