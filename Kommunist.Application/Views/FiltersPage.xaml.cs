using Kommunist.Application.Helpers;
using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views;

public partial class FiltersPage
{
    public FiltersPage()
    {
        InitializeComponent();
        var eventFiltersViewModel = ServiceHelper.Get<EventFiltersViewModel>();
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