using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services.Navigation;

public sealed class MauiNavigationService : INavigationService
{
    public Task GoToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
}
