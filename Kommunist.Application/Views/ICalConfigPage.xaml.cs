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
}