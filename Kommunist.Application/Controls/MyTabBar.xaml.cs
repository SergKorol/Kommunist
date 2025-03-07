using Kommunist.Application.ViewModels;

namespace Kommunist.Application.Controls;

public partial class MyTabBar : ContentPage
{
    ControlTemplate secondaryColorTemplate;
    
    public MyTabBar()
    {
        InitializeComponent();
        secondaryColorTemplate = (ControlTemplate)Resources["SecondaryColorTemplate"];
    }
}