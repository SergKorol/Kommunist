using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views
{
    public partial class ExamplesPage : ContentPage
    {
        public ExamplesPage()
        {
            InitializeComponent();
            BindingContext = new ExamplesViewModel();
        }
    }
}