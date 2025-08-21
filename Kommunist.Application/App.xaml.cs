using Kommunist.Application.Themes;

namespace Kommunist.Application;

public partial class App 
{
    private ResourceDictionary? _themeDictionary;

    public App()
    {
        InitializeComponent();
        if (Current == null) return;

        var savedTheme = Preferences.Get("AppTheme", "Light");

        if (Enum.TryParse(savedTheme, out AppTheme userTheme))
        {
            Current.UserAppTheme = userTheme;
        }

        SetAppThemeResources(Current.RequestedTheme);
        Current.RequestedThemeChanged += OnRequestedThemeChanged;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        Preferences.Set("AppTheme", e.RequestedTheme.ToString());
        SetAppThemeResources(e.RequestedTheme);
    }

    private void SetAppThemeResources(AppTheme theme)
    {
        Resources.MergedDictionaries.Remove(_themeDictionary);
        _themeDictionary = null;

        _themeDictionary = theme == AppTheme.Dark
            ? new DarkTheme()
            : new LightTheme();

        Resources.MergedDictionaries.Add(_themeDictionary);
    }
}