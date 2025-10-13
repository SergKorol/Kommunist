using XCalendar.Core.Models;

namespace Kommunist.Application.Calendar;

public class CalEvent : Event
{
    public int EventId { get; init; }
    public DateTime DateTime { get; set; } = DateTime.Today;

    public string? Location { get; init; }

    public long Start { get; init; }
    public long End { get; init; }
    public Color? Color { get; set; }
    public string? Url { get; init; }
}