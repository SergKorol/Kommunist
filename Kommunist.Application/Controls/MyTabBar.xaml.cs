namespace Kommunist.Application.Controls;

public partial class MyTabBar : ContentPage
{
    
    public MyTabBar()
    {
        InitializeComponent();
        BindingContext = this;
    }
    
    private async void OnTabBarNavigation(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string route)
        {
            await Shell.Current.GoToAsync(route);
        }
    }
}