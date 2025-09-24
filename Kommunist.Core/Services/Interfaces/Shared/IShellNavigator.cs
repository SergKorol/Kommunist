namespace Kommunist.Core.Services.Interfaces.Shared;

public interface IShellNavigator
{
    Task GoToAsync(string route);
}
