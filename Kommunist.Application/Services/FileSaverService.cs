using CommunityToolkit.Maui.Storage;
using Kommunist.Core.Services.Interfaces;
namespace Kommunist.Application.Services;

public class FileSaverService : IFileSaverService
{
    public async Task<FileSaveResult> SaveAsync(string suggestedName, Stream content)
    {
        var result = await FileSaver.Default.SaveAsync(suggestedName, content);
        return new FileSaveResult(result.IsSuccessful, result.FilePath, result.Exception);
    }
}
