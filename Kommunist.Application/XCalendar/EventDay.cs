using XCalendar.Core.Collections;
using XCalendar.Core.Models;

namespace Kommunist.Application.XCalendar;

public class EventDay : CalendarDay<Event>
{
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = [];
}