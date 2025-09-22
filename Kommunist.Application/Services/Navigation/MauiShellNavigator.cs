namespace Kommunist.Application.Services.Navigation;

public sealed class MauiShellNavigator : IShellNavigator
{
    public Task GoToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
}
