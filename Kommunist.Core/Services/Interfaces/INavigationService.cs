namespace Kommunist.Core.Services.Interfaces;

public interface INavigationService
{
    Task GoToAsync(string route);
}
