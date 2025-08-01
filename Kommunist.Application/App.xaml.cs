using Kommunist.Application.Themes;

namespace Kommunist.Application;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        // Apply the theme at startup
        SetAppThemeResources(Current.RequestedTheme);
        
        // Listen for future theme changes
        Current.RequestedThemeChanged += OnRequestedThemeChanged;
    }
    
    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new AppShell());
    }
    
    private void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
    {
        SetAppThemeResources(e.RequestedTheme);
    }

    private void SetAppThemeResources(AppTheme theme)
    {
        if (theme == AppTheme.Dark)
            Resources.MergedDictionaries.Add(new DarkTheme());
        else
            Resources.MergedDictionaries.Add(new LightTheme());
    }
}