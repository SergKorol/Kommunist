using Kommunist.Application.Controls;
using Kommunist.Application.Enums;
using Kommunist.Application.Themes;

namespace Kommunist.Application.Views;

public partial class SettingsPage : MyTabBar
{
    public SettingsPage()
    {
        InitializeComponent();
    }
    
    void OnPickerSelectionChanged(object sender, EventArgs e)
    {
        Picker picker = sender as Picker;
        Theme theme = (Theme)picker.SelectedItem;

        var app = Microsoft.Maui.Controls.Application.Current;
        if (app?.Resources?.MergedDictionaries != null)
        {
            var mergedDictionaries = app.Resources.MergedDictionaries;
            
            // Find and remove only the theme-specific dictionaries (DarkTheme or LightTheme)
            var themesToRemove = mergedDictionaries
                .Where(d => d is DarkTheme || d is LightTheme)
                .ToList();
            
            foreach (var themeDict in themesToRemove)
            {
                mergedDictionaries.Remove(themeDict);
            }

            // Add the selected theme
            switch (theme)
            {
                case Theme.Dark:
                    mergedDictionaries.Add(new DarkTheme());
                    Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Dark;
                    break;
                case Theme.Light:
                default:
                    mergedDictionaries.Add(new LightTheme());
                    Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
                    break;
            }
            
            StatusLabel.Text = $"{theme} theme loaded. Close this page.";
        }
    }

    public async Task Dismiss()
    {
        await Navigation.PopModalAsync();
    }
}