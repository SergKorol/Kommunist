using CommunityToolkit.Maui.Storage;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class ToolkitFileSaverAdapter : IToolkitFileSaverAdapter
{
    public async Task<FileSaveResult> SaveAsync(string suggestedFileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var result = await FileSaver.Default.SaveAsync(suggestedFileName, fileStream, cancellationToken);
        return new FileSaveResult(result.IsSuccessful, result.FilePath, result.Exception);
    }
}