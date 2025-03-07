using Kommunist.Application.Controls;
using Kommunist.Application.ViewModels;


namespace Kommunist.Application.Views;

public partial class EventCalendarPage : MyTabBar
{
    public EventCalendarPage()
    {
        InitializeComponent();
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventCalendarViewModel = serviceProvider?.GetRequiredService<EventCalendarViewModel>();
        BindingContext = eventCalendarViewModel;
    }
}