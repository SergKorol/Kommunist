using System.Reflection;
using Kommunist.Application.Helpers;

namespace Kommunist.Tests.Helpers;

public sealed class ServiceHelperTests : IDisposable
{
    public ServiceHelperTests()
    {
        ResetServiceProvider();
    }

    public void Dispose()
    {
        ResetServiceProvider();
    }

    private static void ResetServiceProvider()
    {
        // Reset the private static field to ensure isolated tests
        var field = typeof(ServiceHelper).GetField("_services", BindingFlags.Static | BindingFlags.NonPublic);
        field?.SetValue(null, null);
    }

    private interface IDummyService
    {
        Guid Id { get; }
    }

    private sealed class DummyService : IDummyService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    [Fact]
    public void Get_ReturnsRegisteredService_WhenInitialized()
    {
        // Arrange
        var expected = new DummyService();
        var services = new ServiceCollection()
            .AddSingleton<IDummyService>(expected)
            .BuildServiceProvider();

        ServiceHelper.Initialize(services);

        // Act
        var resolved = ServiceHelper.Get<IDummyService>();

        // Assert
        Assert.Same(expected, resolved);
    }

    [Fact]
    public void Get_ThrowsInvalidOperationException_WhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();
        ServiceHelper.Initialize(services);

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => ServiceHelper.Get<IDummyService>());

        // Assert
        Assert.Equal($"Service {typeof(IDummyService)} not found.", ex.Message);
    }

    [Fact]
    public void Get_ThrowsInvalidOperationException_WhenNoServicesAreAvailable()
    {
        // Arrange
        ResetServiceProvider(); // Ensure not initialized and no fallback services in test env

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => ServiceHelper.Get<IDummyService>());

        // Assert
        Assert.Equal("MAUI Services are not available.", ex.Message);
    }

    [Fact]
    public void Initialize_ReplacesExistingServiceProvider()
    {
        // Arrange
        var first = new DummyService();
        var firstProvider = new ServiceCollection()
            .AddSingleton<IDummyService>(first)
            .BuildServiceProvider();
        ServiceHelper.Initialize(firstProvider);

        var second = new DummyService();
        var secondProvider = new ServiceCollection()
            .AddSingleton<IDummyService>(second)
            .BuildServiceProvider();

        // Act
        ServiceHelper.Initialize(secondProvider);
        var resolved = ServiceHelper.Get<IDummyService>();

        // Assert
        Assert.Same(second, resolved);
    }
}