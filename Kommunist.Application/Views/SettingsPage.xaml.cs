using Kommunist.Application.Controls;
using Kommunist.Application.Enums;

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

        switch (theme)
        {
            case Theme.Dark:
                Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case Theme.Light:
            default:
                Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
                break;
        }
            
        StatusLabel.Text = $"{theme} theme loaded. Close this page.";
    }
}