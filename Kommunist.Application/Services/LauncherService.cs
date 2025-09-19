using Kommunist.Core.Services.Interfaces;
namespace Kommunist.Application.Services;

public class LauncherService : ILauncherService
{
    public Task OpenAsync(string url)
    {
        return Launcher.OpenAsync(url);
    }
}
