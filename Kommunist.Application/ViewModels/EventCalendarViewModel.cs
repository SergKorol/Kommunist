using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
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
    public Calendar<EventDay> EventCalendar { get; set; } = new Calendar<EventDay>()
    {
        SelectedDates = new ObservableRangeCollection<DateTime>(),
        SelectionType = SelectionType.Single
    };

    public static readonly Random Random = new Random();
    public List<Color> Colors { get; } = new List<Color>()
    {
        Microsoft.Maui.Graphics.Colors.Red,
        Microsoft.Maui.Graphics.Colors.Orange,
        Microsoft.Maui.Graphics.Colors.Yellow,
        Color.FromArgb("#00A000"),
        Microsoft.Maui.Graphics.Colors.Blue,
        Color.FromArgb("#8010E0")
    };

    public ObservableCollection<ServiceEvent> ServiceEvents;
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = new ObservableRangeCollection<CalEvent>();
    public ObservableRangeCollection<CalEvent> SelectedEvents { get; } = new ObservableRangeCollection<CalEvent>();
    #endregion

    #region Commands
    public ICommand NavigateCalendarCommand { get; set; }
    public ICommand ChangeDateSelectionCommand { get; set; }
    public ICommand EventSelectedCommand { get; }
    
    public ICommand OpenIcalConfigCommand { get; }
    #endregion

    private readonly IEventService _eventService;
    private readonly IFileHostingService _fileHostingService;
    private Task _getEvents;

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

        GetEvents(EventCalendar.Days).ConfigureAwait(false);

        foreach (var day in EventCalendar.Days)
        {
            day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
        }
    }
    #endregion

    #region Methods
    private void EventCalendar_DaysUpdated(object sender, EventArgs e)
    {
        var calendar = sender as Calendar<EventDay>;
        if (calendar == null) return;

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

    public void NavigateCalendar(int amount)
    {
        DateTime targetDate;
        if (EventCalendar.NavigatedDate.TryAddMonths(amount, out targetDate))
        {
            EventCalendar.Navigate(targetDate - EventCalendar.NavigatedDate);
        }
        else
        {
            EventCalendar.Navigate(amount > 0 ? TimeSpan.MaxValue : TimeSpan.MinValue);
        }
    }

    public void ChangeDateSelection(DateTime dateTime)
    {
        EventCalendar?.ChangeDateSelection(dateTime);
    }

    private async void OnEventSelected(CalEvent selectedEvent)
    {
        var eventDetailViewModel = new EventCalendarDetailViewModel(_eventService, selectedEvent.EventId, _fileHostingService);;
        await Shell.Current.Navigation.PushAsync(new CalEventDetailPage(eventDetailViewModel));
    }
    
    private async void OpenIcalConfig()
    {
        var navigationParams = new Dictionary<string, object>
        {
            { "SelectedEvents", SelectedEvents.ToList() }
        };
        await Shell.Current.GoToAsync("//ICalConfigPage", navigationParams);
    }
    #endregion

    private async Task GetEvents(ObservableCollection<EventDay> days)
    {
        var dateRange = GetDateRangeForNavigatedMonth(days);
        var serviceEvents = await LoadServiceEvents(dateRange.StartDate, dateRange.EndDate);
        var calEvents = ConvertToCalEvents(serviceEvents);
        
        CalEvents.ReplaceRange(calEvents);
    }

    private (DateTime StartDate, DateTime EndDate) GetDateRangeForNavigatedMonth(ObservableCollection<EventDay> days)
    {
        var daysByNavMonth = days.Where(x => x.DateTime.Date.Month == EventCalendar.NavigatedDate.Date.Month);
        return (daysByNavMonth.First().DateTime, daysByNavMonth.Last().DateTime);
    }

    private async Task<ObservableCollection<ServiceEvent>> LoadServiceEvents(DateTime startDate, DateTime endDate)
    {
        var loadedEvents = await _eventService.LoadEvents(startDate, endDate);
        ServiceEvents = loadedEvents.ToObservableCollection();
        return ServiceEvents;
    }

    private List<CalEvent> ConvertToCalEvents(ObservableCollection<ServiceEvent> serviceEvents)
    {
        return serviceEvents.Select(e => new CalEvent
        {
            EventId = e.Id,
            Title = e.Title,
            Description = BuildEventDescription(e),
            DateTime = e.Start.ToLocalDateTime(),
            Start = e.Start,
            End = e.End,
            Color = Colors[Random.Next(Colors.Count)],
            Url = $"https://wearecommunity.io/events/{e.EventUrl}"
        }).ToList();
    }

    private string BuildEventDescription(ServiceEvent serviceEvent)
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

    public void LoadEvents()
    {
        GetEvents(EventCalendar.Days).ConfigureAwait(false);
    }
}