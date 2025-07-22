using Kommunist.Application.Controls;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class FiltersPage : MyTabBar
{
    public FiltersPage()
    {
        InitializeComponent();
        var serviceProvider = MauiProgram.CreateMauiApp().Services;
        var eventFiltersViewModel = serviceProvider?.GetRequiredService<EventFiltersViewModel>();
        BindingContext = eventFiltersViewModel;
    }
    
    private void BindableObject_OnBindingContextChanged(object sender, EventArgs e)
    {
        if (BindingContext is EventFiltersViewModel viewModel)
        {
            viewModel.LoadFilters();
        }
    }
}