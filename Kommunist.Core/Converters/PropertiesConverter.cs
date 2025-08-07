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
        var type = jsonObject["type"];
        if (type == null) return null;
        var eventType = type.ToObject<PageType>();
        var properties = jsonObject["properties"];

        var eventPage = new EventPage
        {
            Type = eventType,
            Properties = eventType switch
            {
                PageType.Venue => properties?.ToObject<VenueProperties>(),
                PageType.Agenda => properties?.ToObject<AgendaProperties>(),
                PageType.Main => properties?.ToObject<MainProperties>(),
                PageType.EventNavigation => properties?.ToObject<EventNavigationProperties>(),
                PageType.BasicText => properties?.ToObject<BasicTextProperties>(),
                PageType.UnlimitedText => properties?.ToObject<UnlimitedTextProperties>(),
                PageType.StayConnected => properties?.ToObject<StayConnectedProperties>(),
                _ => throw new NotSupportedException($"Unsupported event type: {eventType}")
            }
        };

        return eventPage;
    }
    
    public override void WriteJson(JsonWriter writer, EventPage value, JsonSerializer serializer) { }
}