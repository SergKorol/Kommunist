using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Services.Interfaces.Shared;

namespace Kommunist.Application.Services.Toasts;

public sealed class ToolkitToastFactory : IToolkitToastFactory
{
    public IToolkitToast Make(string message)
    {
        var toolkitToast = Toast.Make(message);
        return new ToolkitToastAdapter(toolkitToast);
    }
}

internal sealed class ToolkitToastAdapter(IToast inner) : IToolkitToast
{
    private readonly IToast _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public Task ShowAsync()
    {
        return _inner.Show();
    }
}
