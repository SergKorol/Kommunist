using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces.Shared;
using Moq;

namespace Kommunist.Tests.Services;

public sealed class MauiNavigationServiceTests
{
    [Fact]
    public void Ctor_WhenShellNavigatorIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MauiNavigationService(null));
    }

    [Fact]
    public void GoToAsync_WithValidRoute_DelegatesToShellNavigatorAndReturnsTask()
    {
        // Arrange
        var shellMock = new Mock<IShellNavigator>(MockBehavior.Strict);
        var sut = new MauiNavigationService(shellMock.Object);
        const string route = "home/details";
        var expectedTask = Task.CompletedTask;

        shellMock
            .Setup(s => s.GoToAsync(route))
            .Returns(expectedTask)
            .Verifiable();

        // Act
        var result = sut.GoToAsync(route);

        // Assert
        Assert.Same(expectedTask, result);
        shellMock.Verify(s => s.GoToAsync(route), Times.Once);
        shellMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GoToAsync_WhenRouteIsNull_ThrowsArgumentNullException()
    {
        var shellMock = new Mock<IShellNavigator>(MockBehavior.Loose);
        var sut = new MauiNavigationService(shellMock.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GoToAsync(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task GoToAsync_WhenRouteIsEmptyOrWhitespace_ThrowsArgumentException(string badRoute)
    {
        var shellMock = new Mock<IShellNavigator>(MockBehavior.Loose);
        var sut = new MauiNavigationService(shellMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.GoToAsync(badRoute));
        Assert.Equal("route", ex.ParamName);
    }

    [Fact]
    public async Task GoToAsync_WhenShellNavigatorFaults_PropagatesException()
    {
        // Arrange
        var shellMock = new Mock<IShellNavigator>(MockBehavior.Strict);
        var sut = new MauiNavigationService(shellMock.Object);
        const string route = "home/details";
        var fault = new InvalidOperationException("boom");

        shellMock
            .Setup(s => s.GoToAsync(route))
            .Returns(Task.FromException(fault));

        // Act + Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GoToAsync(route));
        Assert.Same(fault, ex);
        shellMock.VerifyAll();
    }
}
