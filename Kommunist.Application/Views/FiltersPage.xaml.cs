using Kommunist.Application.Controls;
using Kommunist.Application.Models;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class FiltersPage : MyTabBar
{
    public FiltersPage()
    {
        InitializeComponent();
        // var serviceProvider = MauiProgram.CreateMauiApp().Services;
        // var eventFiltersViewModel = serviceProvider?.GetRequiredService<EventFiltersViewModel>();
        // BindingContext = eventFiltersViewModel;
    }
    
    // private void InitializeCountries()
    // {
    //     // Sample list of countries - replace with your actual data source
    //     var countries = new List<string>
    //     {
    //         "United States",
    //         "Canada",
    //         "United Kingdom",
    //         "Germany",
    //         "France",
    //         "Spain",
    //         "Italy",
    //         "Russia",
    //         "China",
    //         "Japan",
    //         "India",
    //         "Brazil",
    //         "Australia"
    //         // Add more countries as needed
    //     };
    //
    //     CountryPicker.ItemsSource = countries;
    // }
    
    // private void OnApplyFiltersClicked(object sender, System.EventArgs e)
    // {
    //     // Create a filter object with all the values
    //     var filters = new FilterOptions
    //     {
    //         TagFilter = TagSearchEntry.Text,
    //         SpeakerFilter = SpeakerSearchEntry.Text,
    //         CountryFilter = CountryPicker.SelectedItem?.ToString(),
    //         CommunityFilter = CommunitySearchEntry.Text,
    //         OnlineOnly = OnlineSwitch.IsToggled
    //     };
    //
    //     // Handle the filter application
    //     // You might want to call a method from your view model or service
    //     // Example: FilterService.ApplyFilters(filters);
    //
    //     // Notify the user
    //     DisplayAlert("Filters Applied", "Your filters have been applied successfully.", "OK");
    // }
    //
    // private void OnClearFiltersClicked(object sender, System.EventArgs e)
    // {
    //     // Clear all filters
    //     TagSearchEntry.Text = string.Empty;
    //     SpeakerSearchEntry.Text = string.Empty;
    //     CountryPicker.SelectedIndex = -1;
    //     CommunitySearchEntry.Text = string.Empty;
    //     OnlineSwitch.IsToggled = false;
    //
    //     // Handle clearing in your services if needed
    //     // Example: FilterService.ClearFilters();
    //
    //     // Notify the user
    //     DisplayAlert("Filters Cleared", "All filters have been reset.", "OK");
    // }
}