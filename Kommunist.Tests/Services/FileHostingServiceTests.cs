using System.Reflection;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Kommunist.Core.Helpers;
using Kommunist.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kommunist.Tests.Services;

public class FileHostingServiceTests
{
    [Fact]
    public void Ctor_WhenConfigIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration? config = null;

        // Act
        var act = () => new FileHostingService(config);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("config");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task UploadFileAsync_WhenFilePathIsNullOrEmpty_ThrowsArgumentException(string? filePath)
    {
        // Arrange
        var service = CreateServiceWithNoConnectionString();

        // Act
        var act = () => service.UploadFileAsync(filePath, "user@example.com");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task UploadFileAsync_WhenEmailIsNullOrEmpty_ThrowsArgumentException(string? email)
    {
        // Arrange
        using var temp = new TempFile(".txt");
        await File.WriteAllTextAsync(temp.Path, "content");
        var service = CreateServiceWithNoConnectionString();

        // Act
        var path = temp.Path;
        var act = () => service.UploadFileAsync(path, email);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadFileAsync_WhenFileDoesNotExist_LogsErrorAndThrowsFileNotFoundException()
    {
        // Arrange
        var logger = new Mock<ILogger<FileHostingService>>();
        var config = BuildConfigWithConnectionString("UseDevelopmentStorage=true");
        var service = new FileHostingService(config, logger.Object);

        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        // Act
        var act = () => service.UploadFileAsync(nonExistentPath, "user@example.com");

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (Convert.ToString(v) ?? string.Empty).Contains("File not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WhenBlobServiceClientMissing_LogsErrorAndThrowsInvalidOperationException()
    {
        // Arrange
        var logger = new Mock<ILogger<FileHostingService>>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var service = new FileHostingService(config, logger.Object);

        using var temp = new TempFile(".txt");
        await File.WriteAllTextAsync(temp.Path, "content");

        // Act
        var path = temp.Path;
        var act = () => service.UploadFileAsync(path, "user@example.com");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Blob Storage connection string is missing or invalid.");

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (Convert.ToString(v) ?? string.Empty).Contains("Blob Storage connection string is missing or invalid")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(".ics", "text/calendar")]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".txt", "text/plain")]
    [InlineData(".html", "text/html")]
    [InlineData(".htm", "text/html")]
    [InlineData(".json", "application/json")]
    [InlineData(".xml", "application/xml")]
    [InlineData(".bin", "application/octet-stream")]
    public async Task UploadFileAsync_SetsExpectedContentType_AndReturnsBlobUri(string extension, string expectedContentType)
    {
        // Arrange
        var logger = new Mock<ILogger<FileHostingService>>();
        var config = BuildConfigWithConnectionString("UseDevelopmentStorage=true");
        var service = new FileHostingService(config, logger.Object);

        // Mock Azure Blob client chain
        var mockBlobService = new Mock<BlobServiceClient>();
        var mockContainer = new Mock<BlobContainerClient>();
        var mockBlob = new Mock<BlobClient>();

        var testUri = new Uri("https://unit.test/container/file" + extension);
        mockBlob.SetupGet(b => b.Uri).Returns(testUri);

        string? capturedContentType = null;
        mockBlob
            .Setup(b => b.UploadAsync(
                It.IsAny<Stream>(),
                It.Is<BlobUploadOptions>(o => CaptureContentType(o, out capturedContentType)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        mockContainer
            .Setup(c => c.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        string? capturedBlobName = null;
        mockContainer
            .Setup(c => c.GetBlobClient(It.IsAny<string>()))
            .Callback<string>(name => capturedBlobName = name)
            .Returns(mockBlob.Object);

        string? capturedContainerName = null;
        mockBlobService
            .Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
            .Callback<string>(name => capturedContainerName = name)
            .Returns(mockContainer.Object);

        SetPrivateBlobServiceClient(service, mockBlobService.Object);

        using var temp = new TempFile(extension);
        await File.WriteAllTextAsync(temp.Path, "any content");

        const string email = "tester@example.com";
        var expectedContainer = EmailTokenGenerator.EncryptForBlobName(email);
        var expectedBlobName = Path.GetFileName(temp.Path);

        // Act
        var resultUri = await service.UploadFileAsync(temp.Path, email);

        // Assert
        resultUri.Should().Be(testUri.ToString());
        capturedContentType.Should().Be(expectedContentType);

        capturedContainerName.Should().Be(expectedContainer);
        capturedBlobName.Should().Be(expectedBlobName);

        mockContainer.Verify(c => c.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            It.IsAny<IDictionary<string, string>>(),
            It.IsAny<BlobContainerEncryptionScopeOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (Convert.ToString(v) ?? string.Empty).Contains("Uploading file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (Convert.ToString(v) ?? string.Empty).Contains("File uploaded successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UploadFileAsync_WhenUploadThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var logger = new Mock<ILogger<FileHostingService>>();
        var config = BuildConfigWithConnectionString("UseDevelopmentStorage=true");
        var service = new FileHostingService(config, logger.Object);

        var mockBlobService = new Mock<BlobServiceClient>();
        var mockContainer = new Mock<BlobContainerClient>();
        var mockBlob = new Mock<BlobClient>();

        mockBlob
            .Setup(b => b.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<BlobUploadOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("upload failed"));

        mockBlob.SetupGet(b => b.Uri).Returns(new Uri("https://unit.test/container/blob.bin"));

        mockContainer
            .Setup(c => c.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        mockContainer.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(mockBlob.Object);
        mockBlobService.Setup(s => s.GetBlobContainerClient(It.IsAny<string>())).Returns(mockContainer.Object);

        SetPrivateBlobServiceClient(service, mockBlobService.Object);

        using var temp = new TempFile(".bin");
        await File.WriteAllTextAsync(temp.Path, "data");

        // Act
        var path = temp.Path;
        var act = () => service.UploadFileAsync(path, "user@example.com");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("upload failed");

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (Convert.ToString(v) ?? string.Empty).Contains("Failed to upload file")),
                It.Is<InvalidOperationException>(ex => ex.Message == "upload failed"),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helpers

    private static IConfiguration BuildConfigWithConnectionString(string? connectionString)
    {
        var dict = new Dictionary<string, string?>
        {
            ["BlobStorage:ConnectionString"] = connectionString
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static FileHostingService CreateServiceWithNoConnectionString()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new FileHostingService(config);
    }

    private static void SetPrivateBlobServiceClient(FileHostingService service, BlobServiceClient blobServiceClient)
    {
        var field = typeof(FileHostingService).GetField("_blobServiceClient", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull("the service should have a private _blobServiceClient field");
        field?.SetValue(service, blobServiceClient);
    }

    private static bool CaptureContentType(BlobUploadOptions options, out string? captured)
    {
        captured = options.HttpHeaders?.ContentType;
        return true;
    }

    private sealed class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile(string extension)
        {
            var fileName = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), extension);
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            var dir = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using var _ = File.Create(Path);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
