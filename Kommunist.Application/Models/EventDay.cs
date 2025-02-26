using System;
using XCalendar.Core.Collections;
using XCalendar.Core.Interfaces;

namespace Kommunist.Application.Models;

public class EventDay : BaseObservableModel, ICalendarDay
{
    public DateTime DateTime { get; set; }
    public ObservableRangeCollection<CalEvent> CalEvents { get; } = new ObservableRangeCollection<CalEvent>();
    public bool IsSelected { get;set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsInvalid { get; set; }
}