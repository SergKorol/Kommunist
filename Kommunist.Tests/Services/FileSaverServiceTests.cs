using FluentAssertions;
using Kommunist.Application.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Tests.Services;

public class FileSaverServiceTests
{
    [Fact]
    public async Task SaveAsync_WhenUnderlyingSucceeds_ReturnsSuccessfulResultAndMapsValues()
    {
        // Arrange
        var fake = new FakeToolkitFileSaverAdapter();
        var sut = new FileSaverService(fake);

        const string expectedPath = "/tmp/some-file.txt";
        fake.OnSaveAsync = (_, _, _) =>
            Task.FromResult(new FileSaveResult(true, expectedPath, null));
        using var stream = new MemoryStream([1, 2, 3]);
        const string suggestedName = "some-file.txt";

        // Act
        var result = await sut.SaveAsync(suggestedName, stream);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.FilePath.Should().Be(expectedPath);
        result.Exception.Should().BeNull();

        fake.CapturedSuggestedName.Should().Be(suggestedName);
        fake.CapturedContent.Should().BeSameAs(stream);
    }

    [Fact]
    public async Task SaveAsync_WhenUnderlyingFailsWithException_ReturnsFailureAndMapsException()
    {
        // Arrange
        var fake = new FakeToolkitFileSaverAdapter
        {
            OnSaveAsync = (_, _, _) =>
                Task.FromResult(new FileSaveResult(false, null, new InvalidOperationException("boom")))
        };
        var sut = new FileSaverService(fake);
        using var stream = new MemoryStream();

        // Act
        var result = await sut.SaveAsync("fail.txt", stream);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.FilePath.Should().BeNull();
        result.Exception.Should().BeOfType<InvalidOperationException>();
        result.Exception?.Message.Should().Be("boom");
    }

    [Fact]
    public async Task SaveAsync_WhenUnderlyingFailsWithoutException_ReturnsFailureAndNullException()
    {
        // Arrange
        var fake = new FakeToolkitFileSaverAdapter
        {
            OnSaveAsync = (_, _, _) =>
                Task.FromResult(new FileSaveResult(false, null, null))
        };
        var sut = new FileSaverService(fake);
        using var stream = new MemoryStream();

        // Act
        var result = await sut.SaveAsync("fail-no-ex.txt", stream);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.FilePath.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_WhenUnderlyingThrows_ExceptionIsPropagated()
    {
        // Arrange
        var fake = new FakeToolkitFileSaverAdapter
        {
            OnSaveAsync = (_, _, _) => throw new ApplicationException("unexpected")
        };
        var sut = new FileSaverService(fake);

        // Act
        Func<Task> act = () => sut.SaveAsync("throw.txt", Stream.Null);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("unexpected");
    }

    private sealed class FakeToolkitFileSaverAdapter : IToolkitFileSaverAdapter
    {
        public string? CapturedSuggestedName { get; private set; }
        public Stream? CapturedContent { get; private set; }

        public Func<string, Stream, CancellationToken, Task<FileSaveResult>>? OnSaveAsync { get; set; }

        public Task<FileSaveResult> SaveAsync(string suggestedFileName, Stream fileStream, CancellationToken cancellationToken = default)
        {
            CapturedSuggestedName = suggestedFileName;
            CapturedContent = fileStream;

            return OnSaveAsync is not null ? OnSaveAsync(suggestedFileName, fileStream, cancellationToken) : Task.FromResult(new FileSaveResult(true, "/tmp/default.txt", null));
        }
    }
}
