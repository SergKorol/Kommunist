using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kommunist.Application.CustomControls;

public partial class CustomBottomTab : ContentView
{
    public CustomBottomTab()
    {
        InitializeComponent();
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        // Navigate to the Home page
        App.Current.MainPage = new NavigationPage(new Views.EventCalendarPage());
    }

    private void OnCalendarClicked(object sender, EventArgs e)
    {
        // Navigate to the Calendar page
        App.Current.MainPage = new NavigationPage(new Views.EventCalendarPage());
    }

    private void OnFiltersClicked(object sender, EventArgs e)
    {
        // Navigate to the Filters page
        App.Current.MainPage = new NavigationPage(new Views.EventCalendarPage());
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        // Navigate to the Settings page
        App.Current.MainPage = new NavigationPage(new Views.EventCalendarPage());
    }
}