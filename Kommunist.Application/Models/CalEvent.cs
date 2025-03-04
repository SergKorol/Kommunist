using System;
using Microsoft.Maui.Graphics;
using XCalendar.Core.Interfaces;
using XCalendar.Core.Models;

namespace Kommunist.Application.Models;

public class CalEvent : Event
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Today;
    public Color Color { get; set; }
    public string Url { get; set; }
}