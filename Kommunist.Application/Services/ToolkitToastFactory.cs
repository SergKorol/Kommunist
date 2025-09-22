using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class ToolkitToastFactory : IToolkitToastFactory
{
    public IToolkitToast Make(string message)
    {
        var toolkitToast = Toast.Make(message);
        return new ToolkitToastAdapter(toolkitToast);
    }
}

internal sealed class ToolkitToastAdapter : IToolkitToast
{
    private readonly IToast _inner;

    public ToolkitToastAdapter(IToast inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Task ShowAsync()
    {
        return _inner.Show();
    }
}
