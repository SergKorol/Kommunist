using Kommunist.Application.Services.Navigation;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class MauiNavigationService(IShellNavigator shellNavigator) : INavigationService
{
    private readonly IShellNavigator _shellNavigator = shellNavigator ?? throw new ArgumentNullException(nameof(shellNavigator));

    public Task GoToAsync(string route)
    {
        ArgumentNullException.ThrowIfNull(route);
        return string.IsNullOrWhiteSpace(route) ? throw new ArgumentException("Route must not be empty or whitespace.", nameof(route)) : _shellNavigator.GoToAsync(route);
    }
}
