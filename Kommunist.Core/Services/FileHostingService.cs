using System.Net.Mime;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Kommunist.Core.Helpers;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kommunist.Core.Services;

public class FileHostingService : IFileHostingService
{
    private readonly ILogger<FileHostingService>? _logger;
    private readonly BlobServiceClient? _blobServiceClient;

    public FileHostingService(IConfiguration? config, ILogger<FileHostingService>? logger = null)
    {
        var configuration = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        var connectionString = configuration["BlobStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
    }

    public async Task<string> UploadFileAsync(string? filePath, string? email)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrEmpty(email);

        if (!File.Exists(filePath))
        {
            _logger?.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException("File not found", filePath);
        }

        if (_blobServiceClient is null)
        {
            _logger?.LogError("Blob Storage connection string is missing or invalid");
            throw new InvalidOperationException("Blob Storage connection string is missing or invalid.");
        }

        try
        {
            var containerName = EmailTokenGenerator.EncryptForBlobName(email);
            var fileName = Path.GetFileName(filePath);
            var contentType = DetermineContentType(filePath);

            _logger?.LogInformation("Uploading file {FileName} to container {ContainerName}", fileName, containerName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await blobClient.UploadAsync(fileStream, blobUploadOptions);

            var blobUri = blobClient.Uri.ToString();
            _logger?.LogInformation("File uploaded successfully. URI: {BlobUri}", blobUri);

            return blobUri;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to upload file {FilePath}. Error: {ErrorMessage}", filePath, ex.Message);
            throw;
        }
    }

    private static string DetermineContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".ics" => "text/calendar",
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".jpg" or ".jpeg" => MediaTypeNames.Image.Jpeg,
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => MediaTypeNames.Text.Plain,
            ".html" or ".htm" => MediaTypeNames.Text.Html,
            ".json" => MediaTypeNames.Application.Json,
            ".xml" => MediaTypeNames.Application.Xml,
            _ => MediaTypeNames.Application.Octet
        };
    }
}