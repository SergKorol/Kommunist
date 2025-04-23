using System.Windows.Input;

namespace Kommunist.Application.Controls;

public partial class MyTabBar : ContentPage
{
    
    public static readonly BindableProperty NavigateToCommandProperty =
        BindableProperty.Create(nameof(NavigateToCommand), typeof(ICommand), typeof(MyTabBar), null);

    public ICommand NavigateToCommand
    {
        get => (ICommand)GetValue(NavigateToCommandProperty);
        set => SetValue(NavigateToCommandProperty, value);
    }
    
    public MyTabBar()
    {
        InitializeComponent();
        NavigateToCommand = new Command<string>(OnNavigateTo);
    }
    
    private async void OnNavigateTo(string pageName)
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
                break;
        }
    }
    
    private async void OnTabBarNavigation(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string route)
        {
            await Shell.Current.GoToAsync(route);
        }
    }
}