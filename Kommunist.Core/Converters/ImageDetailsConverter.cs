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
        if (reader.TokenType == JsonToken.String)
        {
            return new ImageDetails { Url = reader.Value.ToString() };
        }
        else if (reader.TokenType == JsonToken.StartObject)
        {
            return serializer.Deserialize<ImageDetails>(reader);
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}