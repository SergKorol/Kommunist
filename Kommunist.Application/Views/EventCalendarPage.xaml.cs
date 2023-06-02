using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class EventCalendarPage : ContentPage
{
    public EventCalendarPage()
    {
        InitializeComponent();
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventCalendarViewModel = serviceProvider?.GetRequiredService<EventCalendarViewModel>();
        BindingContext = eventCalendarViewModel;
    }
}