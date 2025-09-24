using FluentAssertions;
using Kommunist.Application.Services;
using Kommunist.Application.Services.Dialog;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Services.Interfaces.Shared;
using Moq;

namespace Kommunist.Tests.Services;

public class PageDialogServiceTests
{
    private const string Title = "Choose";
    private const string Cancel = "Cancel";
    private const string Destruction = "Delete";
    
    [Fact]
    public async Task DisplayActionSheet_ReturnsUnderlyingResult()
    {
        // Arrange
        var buttons = new[] { "A", "B" };
        const string expected = "B";

        var dialogMock = new Mock<IMainPageDialog>(MockBehavior.Strict);
        dialogMock
            .Setup(d => d.DisplayActionSheet(Title, Cancel, Destruction, It.Is<string[]>(a => a.SequenceEqual(buttons))))
            .ReturnsAsync(expected);

        var sut = new PageDialogService(dialogMock.Object);

        // Act
        var result = await sut.DisplayActionSheet(Title, Cancel, Destruction, buttons);

        // Assert
        result.Should().Be(expected);
        dialogMock.VerifyAll();
    }

    [Fact]
    public async Task DisplayActionSheet_ReturnsNull_WhenUnderlyingReturnsNull()
    {
        // Arrange
        var buttons = Array.Empty<string>();

        var dialogMock = new Mock<IMainPageDialog>(MockBehavior.Strict);
        dialogMock
            .Setup(d => d.DisplayActionSheet(Title, Cancel, Destruction, It.Is<string[]>(a => a.SequenceEqual(buttons))))
            .ReturnsAsync((string?)null);

        var sut = new PageDialogService(dialogMock.Object);

        // Act
        var result = await sut.DisplayActionSheet(Title, Cancel, Destruction, buttons);

        // Assert
        result.Should().BeNull();
        dialogMock.VerifyAll();
    }

    [Fact]
    public async Task DisplayActionSheet_PassesParametersThrough()
    {
        // Arrange
        const string title = "Title-123";
        const string cancel = "Cancel-456";
        const string destruction = "Destroy-789";
        var buttons = new[] { "One", "Two", "Three" };

        var called = false;

        var dialogMock = new Mock<IMainPageDialog>(MockBehavior.Strict);
        dialogMock
            .Setup(d => d.DisplayActionSheet(
                It.Is<string>(t => t == title),
                It.Is<string>(c => c == cancel),
                It.Is<string>(x => x == destruction),
                It.Is<string[]>(b => b.SequenceEqual(buttons))))
            .Callback(() => called = true)
            .ReturnsAsync("One");

        var sut = new PageDialogService(dialogMock.Object);

        // Act
        _ = await sut.DisplayActionSheet(title, cancel, destruction, buttons);

        // Assert
        called.Should().BeTrue("the service should forward all parameters to the underlying dialog");
        dialogMock.VerifyAll();
    }

    [Fact]
    public async Task DisplayActionSheet_PropagatesException()
    {
        // Arrange
        const string title = "Error title";
        const string cancel = "Cancel";
        const string destruction = "Destroy";
        var buttons = new[] { "X" };

        var dialogMock = new Mock<IMainPageDialog>(MockBehavior.Strict);
        var expected = new InvalidOperationException("boom");
        dialogMock
            .Setup(d => d.DisplayActionSheet(title, cancel, destruction, It.Is<string[]>(a => a.SequenceEqual(buttons))))
            .ThrowsAsync(expected);

        var sut = new PageDialogService(dialogMock.Object);

        // Act
        var act = async () => await sut.DisplayActionSheet(title, cancel, destruction, buttons);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");
        dialogMock.VerifyAll();
    }
}
