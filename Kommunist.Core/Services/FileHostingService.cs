using System.Net.Http.Headers;
using System.Text.Json;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class FileHostingService(HttpClient httpClient) : IFileHostingService
{
    public async Task<string> UploadFileAsync(string filePath)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://0x0.st");
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead(filePath)), "file", filePath);
        request.Content = content;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var url = await response.Content.ReadAsStringAsync();
        
        return url;
    }
    
    private async Task<string> GetDirectDownloadUrl(string contentId)
    {
        // Step 3: Get file content details (with authentication)
        string detailsUrl = $"https://api.gofile.io/contents/{contentId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, detailsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "oojn42DmhuXu2PpUfOqDvDV4MgtGDrqs");

        HttpResponseMessage detailsResponse = await httpClient.SendAsync(request);
        string detailsResponseBody = await detailsResponse.Content.ReadAsStringAsync();

        using var detailsJson = JsonDocument.Parse(detailsResponseBody);
        if (detailsJson.RootElement.GetProperty("status").GetString() == "ok")
        {
            // Step 4: Request direct link (with authentication)
            string directLinkUrl = $"https://api.gofile.io/contents/{contentId}/directLinks";
            using var directLinkRequest = new HttpRequestMessage(HttpMethod.Post, directLinkUrl);
            directLinkRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "oojn42DmhuXu2PpUfOqDvDV4MgtGDrqs");

            HttpResponseMessage directLinkResponse = await httpClient.SendAsync(directLinkRequest);
            string directLinkResponseBody = await directLinkResponse.Content.ReadAsStringAsync();

            using var directLinkJson = JsonDocument.Parse(directLinkResponseBody);
            if (directLinkJson.RootElement.GetProperty("status").GetString() == "ok")
            {
                return directLinkJson.RootElement.GetProperty("data").GetProperty("directLink").GetString();
            }
        }

        throw new Exception("❌ Failed to get direct download link.");
    }
}