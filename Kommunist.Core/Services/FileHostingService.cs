using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Kommunist.Core.Helpers;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kommunist.Core.Services;

public class FileHostingService(IConfiguration config) : IFileHostingService
{
    public async Task<string> UploadFileAsync(string filePath, string email)
    {
        try
        {
            string containerName = EmailTokenGenerator.EncryptForBlobName(email);
            string fileName = Path.GetFileName(filePath);
            string connectionString = config["BlobStorage:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Blob Storage connection string is missing.");

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            await using var fileStream = File.OpenRead(filePath);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = "text/calendar"
            };
            await blobClient.SetHttpHeadersAsync(blobHttpHeaders);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}