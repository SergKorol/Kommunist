using System.Threading.Tasks;

namespace Kommunist.Core.Services.Interfaces;

public interface IFileHostingService
{
    Task<string> UploadFileAsync(string filePath, string email);
}