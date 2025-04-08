using System;
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
        JObject jsonObject = JObject.Load(reader);

        var eventType = jsonObject["type"]!.ToObject<PageType>();
        var properties = jsonObject["properties"];

        EventPage eventPage = new EventPage();
        eventPage.Type = eventType;

        // Deserialize properties based on the event type
        switch (eventType)
        {
            case PageType.Venue:
                eventPage.Properties = properties?.ToObject<VenueProperties>();
                break;
            case PageType.Agenda:
                eventPage.Properties = properties?.ToObject<AgendaProperties>();
                break;
            case PageType.Main:
                eventPage.Properties = properties?.ToObject<MainProperties>();
                break;
            case PageType.EventNavigation:
                eventPage.Properties = properties?.ToObject<EventNavigationProperties>();
                break;
            case PageType.BasicText:
                eventPage.Properties = properties?.ToObject<BasicTextProperties>();
                break;
            case PageType.UnlimitedText:
                eventPage.Properties = properties?.ToObject<UnlimitedTextProperties>();
                break;
            case PageType.StayConnected:
                eventPage.Properties = properties?.ToObject<StayConnectedProperties>();
                break;
            default:
                throw new NotSupportedException($"Unsupported event type: {eventType}");
        }

        return eventPage;
    }
    
    public override void WriteJson(JsonWriter writer, EventPage value, JsonSerializer serializer) { }
}