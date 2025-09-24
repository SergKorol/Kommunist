using Kommunist.Core.Services.Interfaces.Shared;
// ReSharper disable once RedundantUsingDirective
using CommunityToolkit.Maui.Storage;

namespace Kommunist.Application.Services.File;

public sealed class ToolkitFileSaverAdapter : IToolkitFileSaverAdapter
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<FileSaveResult> SaveAsync(string suggestedFileName, Stream fileStream, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
#if ANDROID || IOS 
        var result = await FileSaver.Default.SaveAsync(suggestedFileName, fileStream, cancellationToken);
        return new FileSaveResult(result.IsSuccessful, result.FilePath, result.Exception);
#else
        throw new PlatformNotSupportedException("File saving is only supported on Android and iOS 14+.");
#endif
    }
}