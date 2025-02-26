using System;
using Microsoft.Maui.Graphics;

namespace Kommunist.Application.Models;

public class CalEvent : BaseObservableModel
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Today;
    public Color Color { get; set; }
    public string Url { get; set; }
}