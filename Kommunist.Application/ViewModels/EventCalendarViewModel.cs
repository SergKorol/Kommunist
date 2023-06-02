using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Core.Entities;
using Kommunist.Core.Services;
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
            SelectionAction = SelectionAction.Modify,
            SelectionType = SelectionType.Single
        };

        public string URL { get; set; } = "https://wearecommunity.io/events/net-talks-8";

        public static readonly Random Random = new Random();
        public List<Color> Colors { get; } = new List<Color>() { Microsoft.Maui.Graphics.Colors.Red, Microsoft.Maui.Graphics.Colors.Orange, Microsoft.Maui.Graphics.Colors.Yellow, Color.FromArgb("#00A000"), Microsoft.Maui.Graphics.Colors.Blue, Color.FromArgb("#8010E0") };
        
        public ObservableCollection<Event> Events;
        public ObservableRangeCollection<CalEvent> CalEvents { get; } = new ObservableRangeCollection<CalEvent>();
        // {
        //     new CalEvent() { Title = "Bowling", Description = "Bowling with friends" },
        //     new CalEvent() { Title = "Swimming", Description = "Swimming with friends" },
        //     new CalEvent() { Title = "Kayaking", Description = "Kayaking with friends" },
        //     new CalEvent() { Title = "Shopping", Description = "Shopping with friends" },
        //     new CalEvent() { Title = "Hiking", Description = "Hiking with friends" },
        //     new CalEvent() { Title = "Kareoke", Description = "Kareoke with friends" },
        //     new CalEvent() { Title = "Dining", Description = "Dining with friends" },
        //     new CalEvent() { Title = "Running", Description = "Running with friends" },
        //     new CalEvent() { Title = "Traveling", Description = "Traveling with friends" },
        //     new CalEvent() { Title = "Clubbing", Description = "Clubbing with friends" },
        //     new CalEvent() { Title = "Learning", Description = "Learning with friends" },
        //     new CalEvent() { Title = "Driving", Description = "Driving with friends" },
        //     new CalEvent() { Title = "Skydiving", Description = "Skydiving with friends" },
        //     new CalEvent() { Title = "Bungee Jumping", Description = "Bungee Jumping with friends" },
        //     new CalEvent() { Title = "Trampolining", Description = "Trampolining with friends" },
        //     new CalEvent() { Title = "Adventuring", Description = "Adventuring with friends" },
        //     new CalEvent() { Title = "Roller Skating", Description = "Rollerskating with friends" },
        //     new CalEvent() { Title = "Ice Skating", Description = "Ice Skating with friends" },
        //     new CalEvent() { Title = "Skateboarding", Description = "Skateboarding with friends" },
        //     new CalEvent() { Title = "Crafting", Description = "Crafting with friends" },
        //     new CalEvent() { Title = "Drinking", Description = "Drinking with friends" },
        //     new CalEvent() { Title = "Playing Games", Description = "Playing Games with friends" },
        //     new CalEvent() { Title = "Canoeing", Description = "Canoeing with friends" },
        //     new CalEvent() { Title = "Climbing", Description = "Climbing with friends" },
        //     new CalEvent() { Title = "Partying", Description = "Partying with friends" },
        //     new CalEvent() { Title = "Relaxing", Description = "Relaxing with friends" },
        //     new CalEvent() { Title = "Exercising", Description = "Exercising with friends" },
        //     new CalEvent() { Title = "Baking", Description = "Baking with friends" },
        //     new CalEvent() { Title = "Skiing", Description = "Skiing with friends" },
        //     new CalEvent() { Title = "Snowboarding", Description = "Snowboarding with friends" },
        //     new CalEvent() { Title = "Surfing", Description = "Surfing with friends" },
        //     new CalEvent() { Title = "Paragliding", Description = "Paragliding with friends" },
        //     new CalEvent() { Title = "Sailing", Description = "Sailing with friends" },
        //     new CalEvent() { Title = "Cooking", Description = "Cooking with friends" }
        // };
        public ObservableRangeCollection<CalEvent> SelectedEvents { get; } = new ObservableRangeCollection<CalEvent>();
        #endregion

        #region Commands
        public ICommand NavigateCalendarCommand { get; set; }
        public ICommand ChangeDateSelectionCommand { get; set; }
        #endregion

        private  readonly IEventService _eventService;
        private Task _getEvents;
        #region Constructors
        public EventCalendarViewModel(IEventService eventService)
        {
            _eventService = eventService;
            GetEvents(EventCalendar.Days).ConfigureAwait(false);
            NavigateCalendarCommand = new Command<int>(NavigateCalendar);
            ChangeDateSelectionCommand = new Command<DateTime>(ChangeDateSelection);

            EventCalendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;
            EventCalendar.DaysUpdated += EventCalendar_DaysUpdated;
            foreach (var day in EventCalendar.Days)
            {
                day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
            }

            // ChangeColorsForEvents();
        }
        #endregion

        #region Methods
        
        private void EventCalendar_DaysUpdated(object sender, EventArgs e)
        {
            var navigatedDate = ((Calendar<EventDay>)sender).NavigatedDate.Date;
            
            if (navigatedDate != DateTime.Today.Date)
            {
               GetEvents(EventCalendar.Days).ConfigureAwait(false);
               // ChangeColorsForEvents();
            }
            
            foreach (var day in EventCalendar.Days)
            {
                day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
            }
        }
        private void SelectedDates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedEvents.ReplaceRange(CalEvents.Where(x => EventCalendar.SelectedDates.Any(y => x.DateTime.Date == y.Date)).OrderByDescending(x => x.DateTime));
        }
        public void NavigateCalendar(int amount)
        {
            if (EventCalendar.NavigatedDate.TryAddMonths(amount, out DateTime targetDate))
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
        #endregion

        // private async Task GetEvents(DateTime startDate, DateTime endDate)
        // {
        //     Events = (await _eventService.LoadEvents(startDate, endDate)).ToObservableCollection();
        //     var calEvents = new List<CalEvent>();
        //     foreach (var e in Events)
        //     {
        //         var location = e.ParticipationFormat.Online ? string.Empty : $"{e.ParticipationFormat.Location}";
        //         var callEvent = new CalEvent { Title = e.Title, Description = $"{e.Start.ToLocalDateTime()} - {e.End.ToLocalDateTime()}, {e.Language}, {location}", DateTime = e.Start.ToLocalDateTime()};
        //         calEvents.Add(callEvent);
        //     }
        //
        //     
        //     CalEvents.ReplaceRange(calEvents);
        //     foreach (CalEvent @event in CalEvents)
        //     {
        //         @event.Color = Colors[Random.Next(6)];
        //     }
        // }
        

        private void ChangeColorsForEvents()
        {
            foreach (CalEvent @event in CalEvents)
            {
                @event.Color = Colors[Random.Next(6)];
            }
        }
        
        // private async Task SetDays()
        // {
        //     await GetEvents(EventCalendar.Days);
        //
        //     foreach (var day in EventCalendar.Days)
        //     {
        //         day.CalEvents.ReplaceRange(CalEvents.Where(x => x.DateTime.Date == day.DateTime.Date));
        //     }
        // }


        private async Task GetEvents(ObservableCollection<EventDay> days)
        {
            var startDate = days.First().DateTime;
            var endDate = days.Last().DateTime;
            var loadedDays = await _eventService.LoadEvents(startDate, endDate);
            Events = loadedDays.ToObservableCollection();
            var calEvents = (from e in Events
                let location = e.ParticipationFormat.Online
                    ? string.Empty
                    : $"{e.ParticipationFormat.Location}"
                select new CalEvent
                {
                    Title = e.Title,
                    Description = $"{e.Start.ToLocalDateTime()} - {e.End.ToLocalDateTime()}, {e.Language}, {location}",
                    DateTime = e.Start.ToLocalDateTime(),
                    Color = Colors[Random.Next(6)],
                    Url = "https://wearecommunity.io/events/net-talks-8"
                }).ToList();
            // calEvents.ForEach(x => x.Color = Microsoft.Maui.Graphics.Colors.Red);   

            CalEvents.ReplaceRange(calEvents);
        }
}