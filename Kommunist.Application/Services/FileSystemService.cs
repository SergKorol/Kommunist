using System.Text;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public class FileSystemService(string appDataDirectory) : IFileSystemService
{
    public FileSystemService() : this(FileSystem.AppDataDirectory)
    {
    }

    public string AppDataDirectory { get; } = appDataDirectory ?? throw new ArgumentNullException(nameof(appDataDirectory));

    public async Task<string> SaveTextAsync(string fileName, string content)
    {
        var filePath = Path.Combine(AppDataDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(content);
        return filePath;
    }
}
