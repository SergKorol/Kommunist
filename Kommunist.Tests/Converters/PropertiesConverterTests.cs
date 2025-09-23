using FluentAssertions;
using Kommunist.Core.Converters;
using Kommunist.Core.Entities;
using Kommunist.Core.Entities.Enums;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Entities.PageProperties.BasicText;
using Kommunist.Core.Entities.PageProperties.EventNavigation;
using Kommunist.Core.Entities.PageProperties.Main;
using Kommunist.Core.Entities.PageProperties.StayConnected;
using Kommunist.Core.Entities.PageProperties.UnlimitedText;
using Kommunist.Core.Entities.PageProperties.Venue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kommunist.Tests.Converters;

public class PropertiesConverterTests
{
    private static JsonSerializerSettings CreateSettings() =>
        new()
        {
            Converters = { new PropertiesConverter() }
        };

    [Theory]
    [InlineData("EventNavigation", typeof(EventNavigationProperties), PageType.EventNavigation)]
    [InlineData("Main", typeof(MainProperties), PageType.Main)]
    [InlineData("BasicText", typeof(BasicTextProperties), PageType.BasicText)]
    [InlineData("Agenda", typeof(AgendaProperties), PageType.Agenda)]
    [InlineData("UnlimitedText", typeof(UnlimitedTextProperties), PageType.UnlimitedText)]
    [InlineData("Venue", typeof(VenueProperties), PageType.Venue)]
    [InlineData("StayConnected", typeof(StayConnectedProperties), PageType.StayConnected)]
    // Case-insensitive parsing
    [InlineData("venue", typeof(VenueProperties), PageType.Venue)]
    [InlineData("VENUE", typeof(VenueProperties), PageType.Venue)]
    // Numeric-as-string parsing should also work
    [InlineData("5", typeof(VenueProperties), PageType.Venue)]
    public void ReadJson_StringTypeToken_DeserializesToExpected(string typeValue, Type expectedPropertiesType, PageType expectedPageType)
    {
        // Arrange
        var json = $"{{\"type\":\"{typeValue}\",\"properties\":{{}}}}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Type.Should().Be(expectedPageType);
        result?.Properties.Should().NotBeNull();
        result?.Properties.Should().BeOfType(expectedPropertiesType);
    }

    [Fact]
    public void ReadJson_NumericTypeToken_DeserializesUsingFallback()
    {
        // Arrange: 5 corresponds to PageType.Venue
        var json = "{\"type\":5,\"properties\":{}}";
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Type.Should().Be(PageType.Venue);
        result?.Properties.Should().NotBeNull();
        result?.Properties.Should().BeOfType<VenueProperties>();
    }

    [Theory]
    [InlineData("{\"properties\":{}}")] // missing type
    [InlineData("{\"type\":null,\"properties\":{}}")] // null type
    public void ReadJson_MissingOrNullType_ReturnsNull(string json)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ReadJson_UnknownStringType_ThrowsJsonSerializationException()
    {
        // Arrange
        var json = "{\"type\":\"NotAValidType\",\"properties\":{}}";
        var settings = CreateSettings();

        // Act
        var act = () => JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        act.Should().Throw<JsonSerializationException>();
    }

    [Fact]
    public void ReadJson_UnsupportedNumericType_ThrowsNotSupportedException()
    {
        // Arrange
        var json = "{\"type\":999,\"properties\":{}}";
        var settings = CreateSettings();

        // Act
        var act = () => JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*Unsupported event type*");
    }

    [Theory]
    [InlineData("{\"type\":\"Venue\"}")]                 // missing properties
    [InlineData("{\"type\":\"Venue\",\"properties\":null}")] // explicit null
    public void ReadJson_PropertiesMissingOrNull_SetsPropertiesNull(string json)
    {
        // Arrange
        var settings = CreateSettings();

        // Act
        var result = JsonConvert.DeserializeObject<EventPage>(json, settings);

        // Assert
        result.Should().NotBeNull();
        result?.Type.Should().Be(PageType.Venue);
        result?.Properties.Should().BeNull();
    }

    [Fact]
    public void WriteJson_NullValue_WritesNull()
    {
        // Arrange
        var settings = CreateSettings();
        EventPage? value = null;

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void WriteJson_PropertiesNull_WritesNullProperties()
    {
        // Arrange
        var settings = CreateSettings();
        var value = new EventPage
        {
            Type = PageType.Venue,
            Properties = null
        };

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        var obj = JObject.Parse(json);
        obj["type"]?.Value<string>().Should().Be("Venue");
        obj["properties"]?.Type.Should().Be(JTokenType.Null);
    }

    [Fact]
    public void WriteJson_PropertiesPresent_MatchesTypeAndSerializesObject()
    {
        // Arrange
        var settings = CreateSettings();
        var value = new EventPage
        {
            Type = PageType.Venue,
            Properties = new VenueProperties()
        };

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        var obj = JObject.Parse(json);
        obj["type"]?.Value<string>().Should().Be("Venue");
        obj["properties"]?.Type.Should().Be(JTokenType.Object);
    }

    [Fact]
    public void WriteJson_MismatchedPropertiesType_SerializesFallback()
    {
        // Arrange
        var settings = CreateSettings();
        var value = new EventPage
        {
            Type = PageType.Venue,
            Properties = new BasicTextProperties()
        };

        // Act
        var json = JsonConvert.SerializeObject(value, settings);

        // Assert
        var obj = JObject.Parse(json);
        obj["type"]?.Value<string>().Should().Be("Venue");
        obj["properties"]?.Type.Should().Be(JTokenType.Object);
    }

    [Fact]
    public void RoundTrip_Venue_WithEmptyProperties_PreservesTypeAndStructure()
    {
        // Arrange
        var settings = CreateSettings();
        var input = "{\"type\":\"Venue\",\"properties\":{}}";

        // Act
        var model = JsonConvert.DeserializeObject<EventPage>(input, settings);
        var output = JsonConvert.SerializeObject(model, settings);
        var jObj = JObject.Parse(output);

        // Assert
        model.Should().NotBeNull();
        model?.Type.Should().Be(PageType.Venue);
        model?.Properties.Should().BeOfType<VenueProperties>();

        jObj["type"]?.Value<string>().Should().Be("Venue");
        jObj["properties"]?.Type.Should().Be(JTokenType.Object);
    }
}
