using System.Threading.Tasks;

namespace Kommunist.Application.Services.Navigation;

public interface IShellNavigator
{
    Task GoToAsync(string route);
}
