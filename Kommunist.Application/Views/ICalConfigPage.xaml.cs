using Kommunist.Application.Helpers;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class CalConfigPage
{
    public CalConfigPage()
    {
        InitializeComponent();
        var eventCalendarViewModel = ServiceHelper.Get<CalConfigViewModel>();
        BindingContext = eventCalendarViewModel;
    }
    
    private void OnAlarmMinutesChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is CalConfigViewModel viewModel && int.TryParse(e.NewTextValue, out var value))
        {
            viewModel.AlarmMinutes = Math.Max(0, value);
        }
    }

    private void OnAlarmEmailChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is CalConfigViewModel viewModel)
        {
            viewModel.Email = e.NewTextValue;
        }
    }
}