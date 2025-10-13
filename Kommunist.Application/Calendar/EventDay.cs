using XCalendar.Core.Collections;
using XCalendar.Core.Models;

namespace Kommunist.Application.Calendar;

public class EventDay : CalendarDay<Event>
{
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = [];
}