using Kommunist.Core.Services.Interfaces;
namespace Kommunist.Application.Services;

public class LauncherService : ILauncherService
{
    private readonly ILauncher _launcher;

    // Default constructor uses the platform default launcher
    public LauncherService() : this(Launcher.Default)
    {
    }

    // Injectable constructor for testing and DI
    public LauncherService(ILauncher launcher)
    {
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
    }

    public Task OpenAsync(string url)
    {
        // Uses the ILauncher extension method that converts string -> Uri and calls ILauncher.OpenAsync(Uri)
        return _launcher.OpenAsync(url);
    }
}
