using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;

namespace Kommunist.Application.Controls;

public partial class MyTabBar
{
    
    private static readonly BindableProperty NavigateToCommandProperty =
        BindableProperty.Create(nameof(NavigateToCommand), typeof(ICommand), typeof(MyTabBar));

    public ICommand NavigateToCommand
    {
        get => (ICommand)GetValue(NavigateToCommandProperty);
        init => SetValue(NavigateToCommandProperty, value);
    }
    
    public MyTabBar()
    {
        InitializeComponent();
        NavigateToCommand = new Command<string>(OnNavigateTo);
    }
    
    private static async void OnNavigateTo(string pageName)
    {
        try
        {
            switch (pageName)
            {
                case "HomePage":
                    await Shell.Current.GoToAsync("//Home");
                    break;
                case "FiltersPage":
                    await Shell.Current.GoToAsync("//Filters");
                    break;
                case "SettingsPage":
                    await Shell.Current.GoToAsync("//Settings");
                    break;
                default:
                    await Shell.Current.GoToAsync("//Home");
                    break;
            }
        }
        catch (Exception e)
        {
            await Toast.Make($"Error navigating to {pageName}: {e}").Show();
        }
    }
}