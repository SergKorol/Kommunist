using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class MauiToastService(IToolkitToastFactory toastFactory) : IToastService
{
    private readonly IToolkitToastFactory _toastFactory = toastFactory ?? throw new ArgumentNullException(nameof(toastFactory));

    public MauiToastService()
        : this(new ToolkitToastFactory())
    {
    }

    public Task ShowAsync(string message)
    {
        var toast = _toastFactory.Make(message);
        return toast.ShowAsync();
    }
}
