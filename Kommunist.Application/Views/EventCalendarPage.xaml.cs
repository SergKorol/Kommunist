using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.ViewModels;


namespace Kommunist.Application.Views;

public partial class EventCalendarPage
{
    public EventCalendarPage()
    {
        InitializeComponent();
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventCalendarViewModel = serviceProvider?.GetRequiredService<EventCalendarViewModel>();
        BindingContext = eventCalendarViewModel;
    }

    private async void VisualElement_OnLoaded(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is EventCalendarViewModel viewModel)
            {
                await viewModel.RefreshCalendarEvents();
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Events weren't refreshed: {ex}").Show();
        }
    }
}