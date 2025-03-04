using System;
using XCalendar.Core.Collections;
using XCalendar.Core.Interfaces;
using XCalendar.Core.Models;

namespace Kommunist.Application.Models;

public class EventDay : CalendarDay<Event>
{
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = new ObservableRangeCollection<CalEvent>();
    // public ObservableRangeCollection<Event> Events { get; set; } = new ObservableRangeCollection<Event>(new ObservableRangeCollection<CalEvent>());
}