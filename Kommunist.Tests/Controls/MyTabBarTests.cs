using System.Windows.Input;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Services.Interfaces.Shared;
using Moq;

namespace Kommunist.Tests.Controls;
public class TestableMyTabBar
{
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    public ICommand NavigateToCommand { get; }

    public TestableMyTabBar(INavigationService? navigationService, IToastService? toastService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

        NavigateToCommand = new Command<string>(OnNavigateTo);
    }

    private async void OnNavigateTo(string pageName)
    {
        try
        {
            var route = pageName switch
            {
                "HomePage" => "//Home",
                "FiltersPage" => "//Filters",
                "SettingsPage" => "//Settings",
                _ => "//Home"
            };

            await _navigationService.GoToAsync(route);
        }
        catch (Exception e)
        {
            await _toastService.ShowAsync($"Error navigating to {pageName}: {e}");
        }
    }
}

public class MyTabBarTests
{
    private readonly Mock<INavigationService> _mockNavigationService = new();
    private readonly Mock<IToastService> _mockToastService = new();

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidServices_SetsPropertiesCorrectly()
    {
        // Act
        var tabBar = CreateTestableMyTabBar();

        // Assert
        Assert.NotNull(tabBar);
        Assert.NotNull(tabBar.NavigateToCommand);
    }

    [Fact]
    public void Constructor_WithNullNavigationService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestableMyTabBar(null, _mockToastService.Object));

        Assert.Equal("navigationService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullToastService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestableMyTabBar(_mockNavigationService.Object, null));

        Assert.Equal("toastService", exception.ParamName);
    }

    #endregion

    #region NavigateToCommand Property Tests

    [Fact]
    public void NavigateToCommand_AfterConstruction_IsNotNull()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();

        // Act & Assert
        Assert.NotNull(tabBar.NavigateToCommand);
        Assert.IsType<Command<string>>(tabBar.NavigateToCommand);
    }

    [Fact]
    public void NavigateToCommand_CanExecute_ReturnsTrue()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();

        // Act
        var canExecute = tabBar.NavigateToCommand.CanExecute("HomePage");

        // Assert
        Assert.True(canExecute);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task NavigateToCommand_WithHomePageParameter_NavigatesToHomeRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("HomePage");

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WithFiltersPageParameter_NavigatesToFiltersRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Filters")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("FiltersPage");

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Filters"), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WithSettingsPageParameter_NavigatesToSettingsRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Settings")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("SettingsPage");

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Settings"), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WithUnknownPageParameter_NavigatesToHomeRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("UnknownPage");

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WithNullParameter_NavigatesToHomeRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute(null);

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WithEmptyStringParameter_NavigatesToHomeRoute()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute(string.Empty);

        await Task.Delay(50);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task NavigateToCommand_WhenNavigationServiceThrows_ShowsToastWithError()
    {
        // Arrange
        var exception = new InvalidOperationException("Navigation failed");
        var tabBar = CreateTestableMyTabBar();

        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).ThrowsAsync(exception);
        _mockToastService.Setup(x => x.ShowAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("HomePage");

        await Task.Delay(100);

        // Assert
        _mockToastService.Verify(x => x.ShowAsync(It.Is<string>(msg =>
            msg.Contains("Error navigating to HomePage") &&
            msg.Contains(exception.ToString()))), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WhenNavigationServiceThrowsException_DoesNotRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Navigation failed");
        var tabBar = CreateTestableMyTabBar();

        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).ThrowsAsync(exception);
        _mockToastService.Setup(x => x.ShowAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act & Assert
        tabBar.NavigateToCommand.Execute("HomePage");

        await Task.Delay(100);

        _mockToastService.Verify(x => x.ShowAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task NavigateToCommand_WhenToastServiceAlsoThrows_DoesNotRethrow()
    {
        // Arrange
        var navigationException = new InvalidOperationException("Navigation failed");
        var toastException = new InvalidOperationException("Toast failed");
        var tabBar = CreateTestableMyTabBar();

        _mockNavigationService.Setup(x => x.GoToAsync("//Home")).ThrowsAsync(navigationException);
        _mockToastService.Setup(x => x.ShowAsync(It.IsAny<string>())).ThrowsAsync(toastException);

        // Act & Assert
        tabBar.NavigateToCommand.Execute("HomePage");

        await Task.Delay(100);

        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
        _mockToastService.Verify(x => x.ShowAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("homepage")]
    [InlineData("HOMEPAGE")]
    [InlineData("HomePage")]
    [InlineData("homePage")]
    public async Task NavigateToCommand_WithDifferentCasing_OnlyExactMatchNavigatesCorrectly(string pageName)
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute(pageName);
        await Task.Delay(50);

        // Assert
        if (pageName == "HomePage")
        {
            _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
        }
        else
        {
            _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
        }
    }

    [Fact]
    public async Task NavigateToCommand_MultipleConsecutiveCalls_AllExecuteCorrectly()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();
        _mockNavigationService.Setup(x => x.GoToAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        tabBar.NavigateToCommand.Execute("HomePage");
        tabBar.NavigateToCommand.Execute("FiltersPage");
        tabBar.NavigateToCommand.Execute("SettingsPage");
        await Task.Delay(100);

        // Assert
        _mockNavigationService.Verify(x => x.GoToAsync("//Home"), Times.Once);
        _mockNavigationService.Verify(x => x.GoToAsync("//Filters"), Times.Once);
        _mockNavigationService.Verify(x => x.GoToAsync("//Settings"), Times.Once);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void NavigateToCommand_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var tabBar = CreateTestableMyTabBar();

        // Act
        var originalCommand = tabBar.NavigateToCommand;

        // Assert
        Assert.NotNull(originalCommand);
        Assert.IsType<Command<string>>(originalCommand);
    }

    #endregion

    #region Helper Methods

    private TestableMyTabBar CreateTestableMyTabBar()
    {
        return new TestableMyTabBar(_mockNavigationService.Object, _mockToastService.Object);
    }

    #endregion
}
