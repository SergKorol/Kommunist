using System.Threading;
using FluentAssertions;
using Moq;
using Kommunist.Application.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Tests.Services;

public sealed class MauiToastServiceTests
{
    [Fact]
    public async Task ShowAsync_CallsFactoryMakeAndToastShowAsync_Once_AndReturnsInnerTask()
    {
        // Arrange
        var message = "Hello";
        var innerTcs = new TaskCompletionSource();

        var toastMock = new Mock<IToolkitToast>(MockBehavior.Strict);
        toastMock.Setup(t => t.ShowAsync())
                 .Returns(innerTcs.Task)
                 .Verifiable();

        var factoryMock = new Mock<IToolkitToastFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.Make(message))
                   .Returns(toastMock.Object)
                   .Verifiable();

        var sut = new MauiToastService(factoryMock.Object);

        // Act
        var returnedTask = sut.ShowAsync(message);

        // Assert
        factoryMock.Verify(f => f.Make(message), Times.Once);
        toastMock.Verify(t => t.ShowAsync(), Times.Once);
        returnedTask.Should().BeSameAs(innerTcs.Task);

        // Complete and await to ensure no deadlocks
        innerTcs.SetResult();
        await returnedTask;
    }

    [Fact]
    public async Task ShowAsync_PropagatesExceptionFromToast()
    {
        // Arrange
        var message = "Crash";
        var exception = new InvalidOperationException("boom");

        var toastMock = new Mock<IToolkitToast>(MockBehavior.Strict);
        toastMock.Setup(t => t.ShowAsync())
                 .Returns(Task.FromException(exception))
                 .Verifiable();

        var factoryMock = new Mock<IToolkitToastFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.Make(message))
                   .Returns(toastMock.Object)
                   .Verifiable();

        var sut = new MauiToastService(factoryMock.Object);

        // Act
        Func<Task> act = () => sut.ShowAsync(message);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("boom");
        factoryMock.Verify(f => f.Make(message), Times.Once);
        toastMock.Verify(t => t.ShowAsync(), Times.Once);
    }

    [Fact]
    public async Task ShowAsync_PropagatesCancellationFromToast()
    {
        // Arrange
        var message = "Cancel";
        var canceledToken = new CancellationToken(true);

        var toastMock = new Mock<IToolkitToast>(MockBehavior.Strict);
        toastMock.Setup(t => t.ShowAsync())
                 .Returns(Task.FromCanceled(canceledToken))
                 .Verifiable();

        var factoryMock = new Mock<IToolkitToastFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.Make(message))
                   .Returns(toastMock.Object)
                   .Verifiable();

        var sut = new MauiToastService(factoryMock.Object);

        // Act
        Func<Task> act = () => sut.ShowAsync(message);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
        factoryMock.Verify(f => f.Make(message), Times.Once);
        toastMock.Verify(t => t.ShowAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Hello, world!")]
    [InlineData("Привіт, світе!")]
    public async Task ShowAsync_PassesThroughMessageVariants(string message)
    {
        // Arrange
        var toastMock = new Mock<IToolkitToast>(MockBehavior.Strict);
        toastMock.Setup(t => t.ShowAsync())
                 .Returns(Task.CompletedTask)
                 .Verifiable();

        var factoryMock = new Mock<IToolkitToastFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.Make(It.Is<string>(m => m == message)))
                   .Returns(toastMock.Object)
                   .Verifiable();

        var sut = new MauiToastService(factoryMock.Object);

        // Act
        await sut.ShowAsync(message);

        // Assert
        factoryMock.Verify(f => f.Make(It.Is<string>(m => m == message)), Times.Once);
        toastMock.Verify(t => t.ShowAsync(), Times.Once);
        factoryMock.VerifyNoOtherCalls();
        toastMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ShowAsync_AllowsNullAndPassesThroughToFactory()
    {
        // Arrange
        string? message = null;

        var toastMock = new Mock<IToolkitToast>(MockBehavior.Strict);
        toastMock.Setup(t => t.ShowAsync())
                 .Returns(Task.CompletedTask)
                 .Verifiable();

        var factoryMock = new Mock<IToolkitToastFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.Make(It.Is<string>(m => m == null)))
                   .Returns(toastMock.Object)
                   .Verifiable();

        var sut = new MauiToastService(factoryMock.Object);

        // Act
        await sut.ShowAsync(message!);

        // Assert
        factoryMock.Verify(f => f.Make(It.Is<string>(m => m == null)), Times.Once);
        toastMock.Verify(t => t.ShowAsync(), Times.Once);
        factoryMock.VerifyNoOtherCalls();
        toastMock.VerifyNoOtherCalls();
    }
}
