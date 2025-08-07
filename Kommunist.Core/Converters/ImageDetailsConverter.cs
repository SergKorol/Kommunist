using Kommunist.Core.Models;
using Newtonsoft.Json;

namespace Kommunist.Core.Converters;

public class ImageDetailsConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ImageDetails);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.String => new ImageDetails { Url = reader.Value?.ToString() ?? "" },
            JsonToken.StartObject => serializer.Deserialize<ImageDetails>(reader),
            _ => null
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}