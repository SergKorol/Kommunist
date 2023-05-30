using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Views
{
    public partial class PlaygroundPage : ContentPage
    {
        public PlaygroundPage()
        {
            InitializeComponent();
            BindingContext = new PlaygroundViewModel();
        }
    }
}
