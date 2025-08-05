using Kommunist.Application.Enums;

namespace Kommunist.Application.Views;

public partial class SettingsPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }
    
    private void OnPickerSelectionChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker) return;
        var theme = (Theme)picker.SelectedItem;

        switch (theme)
        {
            case Theme.Dark:
                if (Microsoft.Maui.Controls.Application.Current != null)
                    Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            case Theme.Light:
            default:
                if (Microsoft.Maui.Controls.Application.Current != null)
                    Microsoft.Maui.Controls.Application.Current.UserAppTheme = AppTheme.Light;
                break;
        }
            
        StatusLabel.Text = $"{theme} theme loaded. Close this page.";
    }
}