using FluentAssertions;
using Kommunist.Core.Converters;
using Kommunist.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kommunist.Tests.Converters;

public class TextItemListConverterTests
{
    private static JsonSerializerSettings CreateSettings() =>
        new()
        {
            Converters = { new TextItemListConverter() }
        };

    [Fact]
    public void ReadJson_NullToken_ReturnsEmptyList()
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>("null", settings);

        // Assert
        result.Should().NotBeNull();
        result?.Should().BeEmpty();
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"\"", "")]
    public void ReadJson_StringToken_ReturnsSingleTextItem(string json, string expectedText)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Should().ContainSingle();
        result?[0].Text.Should().Be(expectedText);
    }

    [Fact]
    public void ReadJson_ObjectToken_ReturnsSingleItem()
    {
        // Arrange
        const string json = "{\"text\":\"hi\",\"type\":\"paragraph\",\"maxLength\":10}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Should().ContainSingle();
        var item = result?.Single();
        item?.Text.Should().Be("hi");
        item?.Type.Should().Be("paragraph");
        item?.MaxLength.Should().Be(10);
    }

    [Fact]
    public void ReadJson_EmptyObjectToken_ReturnsSingleItemWithDefaults()
    {
        // Arrange
        const string json = "{}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        var item = result?.Single();
        item?.Text.Should().BeNull();
        item?.Type.Should().BeNull();
        item?.MaxLength.Should().Be(0);
    }

    [Fact]
    public void ReadJson_ArrayToken_MixedValues_MapsValidAndIgnoresOthers()
    {
        // Arrange
        const string json = "[\"a\", {\"text\":\"b\"}, 123, null, true]";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Select(x => x.Text).Should().Equal("a", "b");
    }

    [Fact]
    public void ReadJson_ArrayToken_NestedArrayAndNulls_Ignored()
    {
        // Arrange
        const string json = "[\"x\", [\"y\"], {\"text\":\"z\"}, null]";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Select(i => i.Text).Should().Equal("x", "z");
    }

    [Fact]
    public void ReadJson_EmptyArray_ReturnsEmptyList()
    {
        // Arrange
        const string json = "[]";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Should().BeEmpty();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("1.23")]
    public void ReadJson_UnsupportedPrimitiveTokens_Throws(string json)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var act = () => JsonConvert.DeserializeObject<List<TextItem>>(json, settings);

        // Assert
        act.Should().Throw<JsonSerializationException>();
    }

    [Fact]
    public void WriteJson_ListOfTextItems_WritesArrayWithObjects()
    {
        // Arrange
        var settings = CreateSettings();
        var value = new List<TextItem>
        {
            new() { Text = "alpha" },
            new() { Text = "beta", MaxLength = 5 }
        };

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        var array = JArray.Parse(json);
        array.Count.Should().Be(2);
        array[0]["text"]?.Value<string>().Should().Be("alpha");
        array[1]["text"]?.Value<string>().Should().Be("beta");
        array[1]["maxLength"]?.Value<int>().Should().Be(5);
    }

    [Fact]
    public void RoundTrip_FromStringInput_ReadsAsList_ThenWritesArray()
    {
        // Arrange
        var settings = CreateSettings();
        const string input = "\"hello\"";

        // Act
        var list = JsonConvert.DeserializeObject<List<TextItem>>(input, settings);
        var output = JsonConvert.SerializeObject(list, settings);

        // Assert
        list.Should().NotBeNull();
        list.Should().ContainSingle();
        list?[0].Text.Should().Be("hello");

        var array = JArray.Parse(output);
        array.Should().HaveCount(1);
        array[0]["text"]?.Value<string>().Should().Be("hello");
    }
}
