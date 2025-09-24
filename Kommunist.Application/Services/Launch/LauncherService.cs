using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services.Launch;

public class LauncherService(ILauncher launcher) : ILauncherService
{
    private readonly ILauncher _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));

    public LauncherService() : this(Launcher.Default)
    {
    }

    public Task OpenAsync(string? url)
    {
        return url != null ? _launcher.OpenAsync(url) : throw new ArgumentNullException(nameof(url));
    }
}
