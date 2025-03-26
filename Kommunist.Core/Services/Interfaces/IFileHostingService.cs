namespace Kommunist.Core.Services.Interfaces;

public interface IFileHostingService
{
    Task<string> UploadFileAsync(string filePath);
}