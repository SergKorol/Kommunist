using System.Windows.Input;
using Kommunist.Application.Services;
using Kommunist.Application.Services.Navigation;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Controls;

public partial class MyTabBar
{
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    private static readonly BindableProperty NavigateToCommandProperty =
        BindableProperty.Create(nameof(NavigateToCommand), typeof(ICommand), typeof(MyTabBar));

    public ICommand NavigateToCommand
    {
        get => (ICommand)GetValue(NavigateToCommandProperty);
        init => SetValue(NavigateToCommandProperty, value);
    }

    // Default constructor used by XAML/runtime
    public MyTabBar()
        : this(new MauiNavigationService(new MauiShellNavigator()), new MauiToastService(), initializeComponent: true)
    {
    }

    // Injectable constructor for unit tests or custom composition
    public MyTabBar(INavigationService navigationService, IToastService toastService, bool initializeComponent = false)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        if (initializeComponent)
        {
            InitializeComponent();
        }

        NavigateToCommand = new Command<string>(OnNavigateTo);
    }

    private async void OnNavigateTo(string pageName)
    {
        try
        {
            var route = pageName switch
            {
                "HomePage" => "//Home",
                "FiltersPage" => "//Filters",
                "SettingsPage" => "//Settings",
                _ => "//Home"
            };

            await _navigationService.GoToAsync(route);
        }
        catch (Exception e)
        {
            await _toastService.ShowAsync($"Error navigating to {pageName}: {e}");
        }
    }
}