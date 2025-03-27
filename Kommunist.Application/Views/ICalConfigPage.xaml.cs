using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
    private void OnInviteesTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is ICalConfigViewModel viewModel)
        {
            viewModel.IsSendEmailEnabled = !string.IsNullOrWhiteSpace(viewModel.Invitees);
        }
    }

    private void OnAlarmMinutesChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is ICalConfigViewModel viewModel && int.TryParse(e.NewTextValue, out int value))
        {
            viewModel.AlarmMinutes = Math.Max(0, value); // Ensure non-negative values
        }
    }
}