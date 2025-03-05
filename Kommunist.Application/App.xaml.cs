using Kommunist.Application.Views;
using Microsoft.Maui.Controls;

namespace Kommunist.Application;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        // Routing.RegisterRoute("CalEventDetailPage", typeof(CalEventDetailPage));
    }
}