namespace Kommunist.Core.Services.Interfaces.Shared;
public record FileSaveResult(bool IsSuccessful, string? FilePath, Exception? Exception);

public interface IFileSaverService
{
    Task<FileSaveResult> SaveAsync(string suggestedName, Stream content);
}
