using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services.File;

public class FileSaverService(IToolkitFileSaverAdapter? adapter = null) : IFileSaverService
{
    private readonly IToolkitFileSaverAdapter _adapter = adapter ?? new ToolkitFileSaverAdapter();

    public Task<FileSaveResult> SaveAsync(string suggestedName, Stream content)
        => _adapter.SaveAsync(suggestedName, content);
}
