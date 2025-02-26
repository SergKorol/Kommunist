using System.Windows.Input;

namespace Kommunist.Application.ViewModels;

public class MainViewModel
{
    public ICommand NavigateCommand { get; }

    public MainViewModel()
    {
        NavigateCommand = new Command<string>(Navigate);
    }

    private async void Navigate(string page)
    {
        switch (page)
        {
            case "Home":
                await Shell.Current.GoToAsync("//HomePage");
                break;
            case "Calendar":
                await Shell.Current.GoToAsync("//CalendarPage");
                break;
            case "Filters":
                await Shell.Current.GoToAsync("//FiltersPage");
                break;
            case "Settings":
                await Shell.Current.GoToAsync("//SettingsPage");
                break;
        }
    }
}