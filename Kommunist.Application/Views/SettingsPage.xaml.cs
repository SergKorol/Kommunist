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

        ICollection<ResourceDictionary> mergedDictionaries = Microsoft.Maui.Controls.Application.Current.Resources.MergedDictionaries;
        if (mergedDictionaries != null)
        {
            mergedDictionaries.Clear();

            switch (theme)
            {
                case Theme.Dark:
                    mergedDictionaries.Add(new DarkTheme());
                    break;
                case Theme.Light:
                default:
                    mergedDictionaries.Add(new LightTheme());
                    break;
            }
            StatusLabel.Text = $"{theme.ToString()} theme loaded. Close this page.";
        }
    }

    public async Task Dismiss()
    {
        await Navigation.PopModalAsync();
    }
}