using Kommunist.Application.Controls;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class ICalConfigPage : MyTabBar
{
    public ICalConfigPage()
    {
        InitializeComponent();
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventCalendarViewModel = serviceProvider?.GetRequiredService<ICalConfigViewModel>();
        BindingContext = eventCalendarViewModel;
    }
    
    private void OnAlarmMinutesChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is ICalConfigViewModel viewModel && int.TryParse(e.NewTextValue, out int value))
        {
            viewModel.AlarmMinutes = Math.Max(0, value);
        }
    }
}