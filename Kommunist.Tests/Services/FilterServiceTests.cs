using System.Text.Json;
using FluentAssertions;
using Kommunist.Core.Models;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Moq;
using Xunit;

namespace Kommunist.Tests.Services;

public class FilterServiceTests
{
    private static FilterOptions CreateSampleFilters() => new()
    {
        TagFilter = "dotnet",
        TagFilters = new() { "maui", "xunit" },
        SpeakerFilter = "Jane Doe",
        SpeakerFilters = new() { "Alice", "Bob" },
        CountryFilters = new() { "US", "DE" },
        CommunityFilter = "DevCommunity",
        CommunityFilters = new() { "Group1" },
        OnlineOnly = true
    };

    [Fact]
    public void Constructor_NoExistingValue_InitializesDefaultFilters()
    {
        // Arrange
        var prefs = new Mock<IAppPreferences>();
        prefs.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>()))
             .Returns((string _, string _) => null);

        // Act
        var service = new FilterService(prefs.Object);

        // Assert
        service.GetFilters().Should().BeEquivalentTo(new FilterOptions());
    }

    [Fact]
    public void Constructor_EmptyStoredValue_InitializesDefaultFilters()
    {
        // Arrange
        var prefs = new Mock<IAppPreferences>();
        prefs.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>()))
             .Returns((string _, string _) => "");

        // Act
        var service = new FilterService(prefs.Object);

        // Assert
        service.GetFilters().Should().BeEquivalentTo(new FilterOptions());
    }

    [Fact]
    public void Constructor_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var prefs = new Mock<IAppPreferences>();
        prefs.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>()))
             .Returns("not-a-json");

        // Act
        var act = () => new FilterService(prefs.Object);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Constructor_NullLiteralJson_TreatedAsDefault()
    {
        // Arrange
        var prefs = new Mock<IAppPreferences>();
        prefs.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>()))
             .Returns("null");

        // Act
        var service = new FilterService(prefs.Object);

        // Assert
        service.GetFilters().Should().BeEquivalentTo(new FilterOptions());
    }

    [Fact]
    public void GetFilters_WithValidStoredJson_ReturnsDeserializedFilters()
    {
        // Arrange
        var expected = CreateSampleFilters();
        var json = JsonSerializer.Serialize(expected);

        var prefs = new Mock<IAppPreferences>();
        prefs.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>()))
             .Returns(json);

        // Act
        var service = new FilterService(prefs.Object);
        var filters = service.GetFilters();

        // Assert
        filters.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SetFilters_Persists_And_SubsequentGetReturnsSame()
    {
        // Arrange
        var memoryPrefs = new InMemoryPreferences();
        var service = new FilterService(memoryPrefs);
        var expected = CreateSampleFilters();

        // Act
        service.SetFilters(expected);
        var restored = service.GetFilters();

        // Assert
        restored.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ClearFilters_RemovesStoredValue_And_ResetsToDefaults()
    {
        // Arrange
        var memoryPrefs = new InMemoryPreferences();
        var service = new FilterService(memoryPrefs);
        service.SetFilters(CreateSampleFilters());

        // Act
        service.ClearFilters();
        var filtersAfterClear = service.GetFilters();

        // Assert
        filtersAfterClear.Should().BeEquivalentTo(new FilterOptions());
    }

    private sealed class InMemoryPreferences : IAppPreferences
    {
        private readonly Dictionary<string, string> _storage = new();

        public string Get(string key, string defaultValue) =>
            _storage.TryGetValue(key, out var v) ? v : defaultValue;

        public void Set(string key, string value) => _storage[key] = value;

        public void Remove(string key) => _storage.Remove(key);
    }
}
