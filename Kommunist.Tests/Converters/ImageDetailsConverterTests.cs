using FluentAssertions;
using Kommunist.Core.Converters;
using Kommunist.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kommunist.Tests.Converters;

public class ImageDetailsConverterTests
{
    private static JsonSerializerSettings CreateSettings() =>
        new()
        {
            Converters = { new ImageDetailsConverter() }
        };

    [Fact]
    public void ReadJson_NullToken_ReturnsNull()
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<ImageDetails>("null", settings);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("\"https://example.com/img.png\"", "https://example.com/img.png")]
    [InlineData("\"\"", "")]
    public void ReadJson_StringToken_ReturnsImageDetails(string json, string expectedUrl)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<ImageDetails>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Url.Should().Be(expectedUrl);
    }

    [Fact]
    public void ReadJson_ObjectToken_ReturnsImageDetails()
    {
        // Arrange
        const string json = "{\"url\":\"https://example.com/a.png\"}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<ImageDetails>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Url.Should().Be("https://example.com/a.png");
    }

    [Fact]
    public void ReadJson_ObjectToken_WithExtraProps_IgnoresUnknowns()
    {
        // Arrange
        const string json = "{\"url\":\"u\",\"foo\":\"bar\"}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<ImageDetails>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Url.Should().Be("u");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("[\"x\"]")]
    public void ReadJson_UnsupportedTokens_ReturnsNull(string json)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<ImageDetails>(json, settings);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WriteJson_NullValue_WritesNull()
    {
        // Arrange
        var settings = CreateSettings();
        ImageDetails? value = null;

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void WriteJson_ObjectValue_WritesObjectWithUrl()
    {
        // Arrange
        var settings = CreateSettings();
        var model = new ImageDetails { Url = "https://example.com/x.png" };

        // Act
        var json = JsonConvert.SerializeObject(model, settings);

        // Assert
        var obj = JObject.Parse(json);
        obj["url"]?.Value<string>().Should().Be("https://example.com/x.png");
        obj.Properties().Select(p => p.Name).Should().ContainSingle().Which.Should().Be("url");
    }

    [Fact]
    public void RoundTrip_FromStringToken_ReadsThenWrites_ObjectJson()
    {
        // Arrange
        var settings = CreateSettings();
        const string inputJson = "\"https://example.com/z.png\"";

        // Act
        var details = JsonConvert.DeserializeObject<ImageDetails>(inputJson, settings);
        var outputJson = JsonConvert.SerializeObject(details, settings);

        // Assert
        details.Should().NotBeNull();
        details?.Url.Should().Be("https://example.com/z.png");
        var obj = JObject.Parse(outputJson);
        obj["url"]?.Value<string>().Should().Be("https://example.com/z.png");
    }
}
