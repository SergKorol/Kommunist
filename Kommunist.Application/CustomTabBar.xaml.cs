using Kommunist.Application.Views;
using Microsoft.Maui.Controls;

namespace Kommunist.Application
{
    public partial class CustomTabBar : ContentView
    {
        public CustomTabBar()
        {
            InitializeComponent();

            // HomeTab.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => NavigateToPage(new EventCalendarPage())) });
            // CalendarTab.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => NavigateToPage(new Views.EventCalendarPage())) });
            // FiltersTab.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => NavigateToPage(new Views.EventCalendarPage())) });
            // SettingsTab.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => NavigateToPage(new Views.EventCalendarPage())) });
        }

        private void NavigateToPage(Page page)
        {
            // Find the MainContent NavigationPage in the AppShell
            if (Parent is Grid grid && grid.Parent is ContentPage contentPage)
            {
                if (contentPage.Parent is Shell shell)
                {
                    var mainContent = shell.FindByName<NavigationPage>("MainContent");
                    if (mainContent != null)
                    {
                        // Navigate to the new page
                        mainContent.PushAsync(page);
                    }
                }
            }
        }
    }
}