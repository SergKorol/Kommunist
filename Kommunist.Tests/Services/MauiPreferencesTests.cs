using FluentAssertions;
using Kommunist.Core.Services;

namespace Kommunist.Tests.Services;

public class MauiPreferencesTests
{
    private static MauiPreferences CreateWithDictionaryBackend()
    {
        var store = new Dictionary<string, string>();

        return new MauiPreferences(
            (key, defaultValue) => store.GetValueOrDefault(key, defaultValue),
            (key, value) => store[key] = value,
            key => store.Remove(key));
    }

    [Fact]
    public void Get_ReturnsDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var sut = CreateWithDictionaryBackend();

        // Act
        var result = sut.Get("missing-key", "default-value");

        // Assert
        result.Should().Be("default-value");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        // Arrange
        var sut = CreateWithDictionaryBackend();

        // Act
        sut.Set("username", "alice");
        var result = sut.Get("username", "default");

        // Assert
        result.Should().Be("alice");
    }

    [Fact]
    public void Remove_ThenGet_ReturnsDefault()
    {
        // Arrange
        var sut = CreateWithDictionaryBackend();

        // Act
        sut.Set("token", "abc123");
        sut.Remove("token");
        var result = sut.Get("token", "fallback");

        // Assert
        result.Should().Be("fallback");
    }

    [Fact]
    public void Set_OverwritesExistingValue()
    {
        // Arrange
        var sut = CreateWithDictionaryBackend();

        // Act
        sut.Set("theme", "light");
        sut.Set("theme", "dark");
        var result = sut.Get("theme", "default");

        // Assert
        result.Should().Be("dark");
    }

    [Fact]
    public void Get_UsesInjectedDelegate_AndPropagatesParametersAndReturnValue()
    {
        // Arrange
        string? capturedKey = null;
        string? capturedDefault = null;

        var sut = new MauiPreferences(
            (key, @default) =>
            {
                capturedKey = key;
                capturedDefault = @default;
                return "computed-value";
            },
            (_, _) => { },
            _ => { });

        // Act
        var result = sut.Get("some-key", "some-default");

        // Assert
        capturedKey.Should().Be("some-key");
        capturedDefault.Should().Be("some-default");
        result.Should().Be("computed-value");
    }

    [Fact]
    public void Set_UsesInjectedDelegate_AndPropagatesParameters()
    {
        // Arrange
        string? capturedKey = null;
        string? capturedValue = null;

        var sut = new MauiPreferences(
            (_, @default) => @default,
            (key, value) =>
            {
                capturedKey = key;
                capturedValue = value;
            },
            _ => { });

        // Act
        sut.Set("api-url", "https://example.test");

        // Assert
        capturedKey.Should().Be("api-url");
        capturedValue.Should().Be("https://example.test");
    }

    [Fact]
    public void Remove_UsesInjectedDelegate_AndPropagatesKey()
    {
        // Arrange
        string? capturedKey = null;

        var sut = new MauiPreferences(
            (_, @default) => @default,
            (_, _) => { },
            key => capturedKey = key);

        // Act
        sut.Remove("obsolete-key");

        // Assert
        capturedKey.Should().Be("obsolete-key");
    }
}
