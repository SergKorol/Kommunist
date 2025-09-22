using Kommunist.Application.Services;
using Moq;

namespace Kommunist.Tests.Services
{
    public class LauncherServiceTests
    {
        [Fact]
        public async Task OpenAsync_DelegatesToILauncher_WithSameUrl()
        {
            // Arrange
            var url = "https://example.com/path?x=1";
            var expectedUri = new Uri(url);

            var launcherMock = new Mock<ILauncher>(MockBehavior.Strict);
            launcherMock
                .Setup(l => l.OpenAsync(It.Is<Uri>(u => u == expectedUri)))
                .ReturnsAsync(true)
                .Verifiable();

            var sut = new LauncherService(launcherMock.Object);

            // Act
            await sut.OpenAsync(url);

            // Assert
            launcherMock.Verify(l => l.OpenAsync(It.IsAny<Uri>()), Times.Once);
            launcherMock.Verify();
        }

        [Fact]
        public async Task OpenAsync_PropagatesException_FromILauncher()
        {
            // Arrange
            var url = "https://example.com";

            var launcherMock = new Mock<ILauncher>(MockBehavior.Strict);
            launcherMock
                .Setup(l => l.OpenAsync(It.IsAny<Uri>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var sut = new LauncherService(launcherMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.OpenAsync(url));
            launcherMock.Verify(l => l.OpenAsync(It.IsAny<Uri>()), Times.Once);
        }

        [Fact]
        public async Task OpenAsync_NullUrl_ThrowsArgumentNullException()
        {
            // Arrange
            var launcherMock = new Mock<ILauncher>(MockBehavior.Strict);
            var sut = new LauncherService(launcherMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.OpenAsync(null!));

            // Ensure ILauncher wasn't called
            launcherMock.Verify(l => l.OpenAsync(It.IsAny<Uri>()), Times.Never);
        }

        [Theory]
        [InlineData("not a uri")]
        [InlineData(" ")]
        [InlineData("")]
        public async Task OpenAsync_InvalidUrl_ThrowsUriFormatException(string invalidUrl)
        {
            // Arrange
            var launcherMock = new Mock<ILauncher>(MockBehavior.Strict);
            var sut = new LauncherService(launcherMock.Object);

            // Act + Assert
            await Assert.ThrowsAnyAsync<UriFormatException>(() => sut.OpenAsync(invalidUrl));

            // Ensure ILauncher wasn't called since parsing failed
            launcherMock.Verify(l => l.OpenAsync(It.IsAny<Uri>()), Times.Never);
        }
    }
}
