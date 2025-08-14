using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core.Extensions;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Application.Views;
using Kommunist.Core.Entities;
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

    private static readonly Random Random = new();

    private List<Color> Colors { get; } =
    [
        Microsoft.Maui.Graphics.Colors.Red,
        Microsoft.Maui.Graphics.Colors.Orange,
        Microsoft.Maui.Graphics.Colors.Yellow,
        Color.FromArgb("#00A000"),
        Microsoft.Maui.Graphics.Colors.Blue,
        Color.FromArgb("#8010E0")
    ];

    private ObservableCollection<ServiceEvent> _serviceEvents;
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = [];
    public ObservableRangeCollection<CalEvent> SelectedEvents { get; } = [];
    #endregion

    #region Commands
    public ICommand NavigateCalendarCommand { get; set; }
    public ICommand ChangeDateSelectionCommand { get; set; }
    public ICommand EventSelectedCommand { get; }
    
    public ICommand OpenIcalConfigCommand { get; }
    #endregion

    private readonly IEventService _eventService;
    private readonly IFileHostingService _fileHostingService;

    #region Constructors
    public EventCalendarViewModel(IEventService eventService, IFileHostingService fileHostingService)
    {
        _eventService = eventService;
        _fileHostingService = fileHostingService;
        NavigateCalendarCommand = new Command<int>(NavigateCalendar);
        ChangeDateSelectionCommand = new Command<DateTime>(ChangeDateSelection);
        EventSelectedCommand = new Command<CalEvent>(OnEventSelected);

        EventCalendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;
        EventCalendar.DaysUpdated += EventCalendar_DaysUpdated;
        
        OpenIcalConfigCommand = new Command(OpenIcalConfig);
    }
    #endregion

    #region Methods
    private void EventCalendar_DaysUpdated(object sender, EventArgs e)
    {
        if (sender is not Calendar<EventDay> calendar) return;

        if (CalEvents.All(x => x.DateTime.Date != calendar.NavigatedDate.Date))
        {
            GetEvents(EventCalendar.Days).ConfigureAwait(false);
        }

        foreach (var day in EventCalendar.Days)
        {
            day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
        }
    }

    private void SelectedDates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedEvents.ReplaceRange(CalEvents
            .Where(x => EventCalendar.SelectedDates.Any(y => x.DateTime.Date == y.Date))
            .OrderByDescending(x => x.DateTime));
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
        EventCalendar?.ChangeDateSelection(dateTime);
    }

    private async void OnEventSelected(CalEvent selectedEvent)
    {
        try
        {
            var eventDetailViewModel = new EventCalendarDetailViewModel(_eventService, selectedEvent.EventId, _fileHostingService);
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
    #endregion

    private async Task GetEvents(ObservableCollection<EventDay> days)
    {
        var dateRange = GetDateRangeForNavigatedMonth(days);
        var serviceEvents = await LoadServiceEvents(dateRange.StartDate, dateRange.EndDate);
        var calEvents = ConvertToCalEvents(serviceEvents);
        
        CalEvents.ReplaceRange(calEvents);
    }

    public async Task RefreshCalendarEvents()
    {
        try
        {
            // Get fresh events with current filters applied
            await GetEvents(EventCalendar.Days);
        
            // Update all calendar days with the new filtered events
            foreach (var day in EventCalendar.Days)
            {
                day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
            }
        
            // Update selected events if any dates are currently selected
            if (EventCalendar.SelectedDates.Any())
            {
                SelectedEvents.ReplaceRange(CalEvents
                    .Where(x => EventCalendar.SelectedDates.Any(y => x.DateTime.Date == y.Date))
                    .OrderByDescending(x => x.DateTime));
            }
        
            // Trigger property changed notifications to update UI
            OnPropertyChanged(nameof(CalEvents));
            OnPropertyChanged(nameof(SelectedEvents));
        }
        catch (Exception ex)
        {
            // Handle any errors that might occur during refresh
            // Log the error or show a user-friendly message
            System.Diagnostics.Debug.WriteLine($"Error refreshing calendar events: {ex.Message}");
        
            // Optionally show a toast or alert to the user
            await Toast.Make("Failed to refresh events. Please try again.").Show();
        }
    }

    private (DateTime StartDate, DateTime EndDate) GetDateRangeForNavigatedMonth(ObservableCollection<EventDay> days)
    {
        var daysByNavMonth = days.Where(x => x.DateTime.Date.Month == EventCalendar.NavigatedDate.Date.Month).ToList();
        return (daysByNavMonth.First().DateTime, daysByNavMonth.Last().DateTime);
    }

    private async Task<ObservableCollection<ServiceEvent>> LoadServiceEvents(DateTime startDate, DateTime endDate)
    {
        var loadedEvents = await _eventService.LoadEvents(startDate, endDate);
        _serviceEvents = loadedEvents.ToObservableCollection();
        return _serviceEvents;
    }

    private List<CalEvent> ConvertToCalEvents(ObservableCollection<ServiceEvent> serviceEvents)
    {
        return serviceEvents.Select(e => new CalEvent
        {
            EventId = e.Id,
            Title = e.Title,
            Location = e.ParticipationFormat.Online ? string.Empty : e.ParticipationFormat.Location,
            Description = BuildEventDescription(e),
            DateTime = e.Start.ToLocalDateTime(),
            Start = e.Start,
            End = e.End,
            Color = Colors[Random.Next(Colors.Count)],
            Url = $"https://wearecommunity.io/events/{e.EventUrl}"
        }).ToList();
    }

    private static string BuildEventDescription(ServiceEvent serviceEvent)
    {
        var startDateFormatted = serviceEvent.Start.ToLocalDateTime().ToString("dd.MM.yyyy");
        var endDateFormatted = serviceEvent.End.ToLocalDateTime().ToString("dd.MM.yyyy");
        var languages = string.Join("/", serviceEvent.Languages).ToUpper();
        var location = serviceEvent.ParticipationFormat.Online ? string.Empty : serviceEvent.ParticipationFormat.Location;
        
        var baseDescription = $"{startDateFormatted} - {endDateFormatted}, {languages}";
        
        return string.IsNullOrWhiteSpace(location) 
            ? baseDescription 
            : $"{baseDescription}, {location}";
    }
}