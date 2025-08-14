using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.Enums;

namespace Kommunist.Application.Views;

public partial class SettingsPage
{
    private CancellationTokenSource _statusCts;

    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnPickerSelectionChanged(object sender, EventArgs e)
    {
        try
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

            if (_statusCts is not null)
            {
                await _statusCts.CancelAsync();
                _statusCts.Dispose();
            }

            _statusCts = new CancellationTokenSource();
            var token = _statusCts.Token;

            StatusLabel.Text = $"{theme} theme loaded.";

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                if (!token.IsCancellationRequested)
                    StatusLabel.Text = string.Empty;
            }
            catch (TaskCanceledException)
            {
                await Toast.Make("Theme change cancelled.").Show(token);
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error changing theme: {ex}").Show();
        }
    }
}