using Kommunist.Application.Controls;
using Kommunist.Application.ViewModels;


namespace Kommunist.Application.Views;

public partial class EventCalendarPage : MyTabBar
{
    public EventCalendarPage()
    {
        InitializeComponent();
        var theme = Microsoft.Maui.Controls.Application.Current.RequestedTheme;
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventCalendarViewModel = serviceProvider?.GetRequiredService<EventCalendarViewModel>();
        BindingContext = eventCalendarViewModel;
    }

    private async void VisualElement_OnLoaded(object sender, EventArgs e)
    {
        if (BindingContext is EventCalendarViewModel viewModel)
        {
            await viewModel.RefreshCalendarEvents();
        }
    }
}