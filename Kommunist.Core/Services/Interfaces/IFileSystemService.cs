namespace Kommunist.Core.Services.Interfaces;
public interface IFileSystemService
{
    string AppDataDirectory { get; }
    Task<string> SaveTextAsync(string fileName, string content);
}
