using System.Security.Cryptography;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Kommunist.Core.Helpers;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class FileHostingService() : IFileHostingService
{
    // public async Task<string> UploadFileAsync(string filePath)
    // {
    //     var client = new HttpClient();
    //     var request = new HttpRequestMessage(HttpMethod.Post, "https://0x0.st");
    //     var content = new MultipartFormDataContent();
    //     content.Add(new StreamContent(File.OpenRead(filePath)), "file", "events.ics");
    //     content.Add(new StringContent("720"), "expires");
    //     request.Content = content;
    //     var response = await client.SendAsync(request);
    //     response.EnsureSuccessStatusCode();
    //     
    //     var url = await response.Content.ReadAsStringAsync();
    //     
    //     return url;
    // }


    public async Task<string> UploadFileAsync(string filePath, string email)
    {
        

        try
        {
            string containerName = EmailTokenGenerator.EncryptForBlobName(email);
            string fileName = Path.GetFileName(filePath);
            string connectionString = "";

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