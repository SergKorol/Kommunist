using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class MauiToastService : IToastService
{
    private readonly IToolkitToastFactory _toastFactory;

    // Keeps existing behavior for production code and existing call sites
    public MauiToastService()
        : this(new ToolkitToastFactory())
    {
    }

    // Testable constructor
    public MauiToastService(IToolkitToastFactory toastFactory)
    {
        _toastFactory = toastFactory ?? throw new ArgumentNullException(nameof(toastFactory));
    }

    public Task ShowAsync(string message)
    {
        var toast = _toastFactory.Make(message);
        return toast.ShowAsync();
    }
}
