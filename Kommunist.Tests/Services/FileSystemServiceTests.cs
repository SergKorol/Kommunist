using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kommunist.Application.Services;
using Xunit;

namespace Kommunist.Tests.Services;

public class FileSystemServiceTests
{
    private static string UniqueFileName(string prefix = "fs-test") => $"{prefix}-{Guid.NewGuid():N}.txt";
    private static string UniqueTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"kom-files-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void AppDataDirectory_ShouldReturnExistingDirectory()
    {
        // Arrange
        var tempDir = UniqueTempDir();
        try
        {
            var sut = new FileSystemService(tempDir);

            // Act
            var path = sut.AppDataDirectory;

            // Assert
            path.Should().Be(tempDir);
            path.Should().NotBeNullOrWhiteSpace();
            Directory.Exists(path).Should().BeTrue("AppDataDirectory should point to an existing directory");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveTextAsync_ShouldWriteContentAndReturnExpectedFullPath()
    {
        // Arrange
        var tempDir = UniqueTempDir();
        var sut = new FileSystemService(tempDir);
        var fileName = UniqueFileName();
        var expectedPath = Path.Combine(sut.AppDataDirectory, fileName);
        var content = "Hello, こんにちは 👋";

        // Act
        string returnedPath = await sut.SaveTextAsync(fileName, content);

        try
        {
            // Assert
            returnedPath.Should().Be(expectedPath);
            File.Exists(returnedPath).Should().BeTrue();
            var written = File.ReadAllText(returnedPath);
            written.Should().Be(content);
        }
        finally
        {
            if (File.Exists(expectedPath))
                File.Delete(expectedPath);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveTextAsync_ShouldOverwriteExistingFile()
    {
        // Arrange
        var tempDir = UniqueTempDir();
        var sut = new FileSystemService(tempDir);
        var fileName = UniqueFileName();
        var targetPath = Path.Combine(sut.AppDataDirectory, fileName);

        File.WriteAllText(targetPath, "OLD CONTENT");

        // Act
        try
        {
            string returnedPath = await sut.SaveTextAsync(fileName, "NEW CONTENT");

            // Assert
            returnedPath.Should().Be(targetPath);
            File.Exists(targetPath).Should().BeTrue();
            File.ReadAllText(targetPath).Should().Be("NEW CONTENT");
        }
        finally
        {
            if (File.Exists(targetPath))
                File.Delete(targetPath);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveTextAsync_WithEmptyContent_CreatesEmptyTextFile()
    {
        // Arrange
        var tempDir = UniqueTempDir();
        var sut = new FileSystemService(tempDir);
        var fileName = UniqueFileName();
        var expectedPath = Path.Combine(sut.AppDataDirectory, fileName);

        // Act
        string returnedPath = await sut.SaveTextAsync(fileName, string.Empty);

        try
        {
            // Assert
            returnedPath.Should().Be(expectedPath);
            File.Exists(expectedPath).Should().BeTrue();

            // Validate textual content is empty (independent of BOM presence)
            var text = File.ReadAllText(expectedPath);
            text.Should().BeEmpty();
        }
        finally
        {
            if (File.Exists(expectedPath))
                File.Delete(expectedPath);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SaveTextAsync_WithInvalidFileName_Throws()
    {
        // Arrange
        var tempDir = UniqueTempDir();
        var sut = new FileSystemService(tempDir);

        // Build a filename that is invalid across platforms where possible.
        // Otherwise, force a non-existent subdirectory to trigger an error.
        var invalidChars = Path.GetInvalidFileNameChars();
        char? invalidChar = invalidChars.FirstOrDefault(c =>
            c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar);

        string fileName = invalidChar.HasValue && invalidChar.Value != '\0'
            ? $"bad{invalidChar.Value}name.txt"
            : $"no_such_dir{Path.DirectorySeparatorChar}file.txt";

        // Act
        Func<Task> act = async () => await sut.SaveTextAsync(fileName, "content");

        // Assert
        await act.Should().ThrowAsync<Exception>("an invalid filename or missing directory should cause an exception");

        // Cleanup
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }
}
