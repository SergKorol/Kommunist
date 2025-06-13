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

    private void VisualElement_OnLoaded(object sender, EventArgs e)
    {
        if (BindingContext is EventCalendarViewModel viewModel)
        {
            viewModel.LoadEvents();
        }
    }
}