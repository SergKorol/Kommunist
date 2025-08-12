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

namespace Kommunist.Core.Converters;

public class PropertiesConverter : JsonConverter<EventPage>
{
    public override EventPage ReadJson(JsonReader reader, Type objectType, EventPage existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        var typeToken = jsonObject["type"];
        if (typeToken == null || typeToken.Type == JTokenType.Null)
        {
            return null;
        }

        PageType eventType;
        if (typeToken.Type == JTokenType.String &&
            Enum.TryParse(typeToken.ToString(), ignoreCase: true, out PageType parsed))
        {
            eventType = parsed;
        }
        else
        {
            // Fallback to default conversion (supports numeric enums as well)
            eventType = typeToken.ToObject<PageType>(serializer);
        }

        var propertiesToken = jsonObject["properties"];

        var eventPage = new EventPage
        {
            Type = eventType,
            Properties = eventType switch
            {
                PageType.Venue => propertiesToken?.ToObject<VenueProperties>(serializer),
                PageType.Agenda => propertiesToken?.ToObject<AgendaProperties>(serializer),
                PageType.Main => propertiesToken?.ToObject<MainProperties>(serializer),
                PageType.EventNavigation => propertiesToken?.ToObject<EventNavigationProperties>(serializer),
                PageType.BasicText => propertiesToken?.ToObject<BasicTextProperties>(serializer),
                PageType.UnlimitedText => propertiesToken?.ToObject<UnlimitedTextProperties>(serializer),
                PageType.StayConnected => propertiesToken?.ToObject<StayConnectedProperties>(serializer),
                _ => throw new NotSupportedException($"Unsupported event type: {eventType}")
            }
        };

        return eventPage;
    }

    public override void WriteJson(JsonWriter writer, EventPage value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("type");
        writer.WriteValue(value.Type.ToString());

        writer.WritePropertyName("properties");
        if (value.Properties is null)
        {
            writer.WriteNull();
        }
        else
        {
            var targetType = GetPropertiesType(value.Type);
            if (targetType != null && targetType.IsInstanceOfType(value.Properties))
            {
                serializer.Serialize(writer, value.Properties, targetType);
            }
            else
            {
                serializer.Serialize(writer, value.Properties);
            }
        }

        writer.WriteEndObject();
    }

    private static Type GetPropertiesType(PageType pageType) =>
        pageType switch
        {
            PageType.Venue => typeof(VenueProperties),
            PageType.Agenda => typeof(AgendaProperties),
            PageType.Main => typeof(MainProperties),
            PageType.EventNavigation => typeof(EventNavigationProperties),
            PageType.BasicText => typeof(BasicTextProperties),
            PageType.UnlimitedText => typeof(UnlimitedTextProperties),
            PageType.StayConnected => typeof(StayConnectedProperties),
            _ => null
        };
}