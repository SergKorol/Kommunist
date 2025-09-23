namespace Kommunist.Core.Services.Interfaces;

public interface IToolkitFileSaverAdapter
{
    Task<FileSaveResult> SaveAsync(string suggestedFileName, Stream fileStream, CancellationToken cancellationToken = default);
}
