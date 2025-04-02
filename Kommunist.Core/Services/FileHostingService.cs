using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class FileHostingService(HttpClient httpClient) : IFileHostingService
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
    


    public async Task<string> UploadFileAsync(string filePath)
    {
        try
        {
            string bucketName = "ical-events.firebasestorage.app";
            // var bucketName = Guid.NewGuid().ToString();
            string objectName = Path.GetFileName(filePath);
        
            // Path to your service account JSON file
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string credentialsPath = Path.Combine(baseDirectory, "service-account.json");
            
            var path = await FileSystem.OpenAppPackageFileAsync("service-account.json");
        
            // Create Storage client with explicit credentials
            StorageClient client = await StorageClient.CreateAsync(
                GoogleCredential.FromFile(credentialsPath));
            
            var existingBucket = await client.GetBucketAsync(bucketName);
            if (existingBucket == null)
            {
                var bucket = await client.CreateBucketAsync("ical-events", bucketName);
            }


            await using var fileStream = File.OpenRead(filePath);
            var uploadedObject = await client.UploadObjectAsync(
                bucketName,
                objectName,
                "application/octet-stream",
                fileStream
            );
            
            var file = await client.GetObjectAsync(bucketName, objectName);
            // Get public download URL
            string downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media";
            
            return downloadUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex.Message}");
            throw;
        }
    }
}