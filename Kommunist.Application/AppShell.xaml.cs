using Kommunist.Application.ViewModels;
using Kommunist.Application.Views;
using Microsoft.Maui.Controls;

namespace Kommunist.Application;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("Calendar", typeof(EventCalendarPage));
        Routing.RegisterRoute("Home", typeof(EventCalendarPage));
        Routing.RegisterRoute("Cal", typeof(EventCalendarPage));
        Routing.RegisterRoute("Filters", typeof(EventCalendarPage));
        Routing.RegisterRoute("Settings", typeof(EventCalendarPage));
    }
}