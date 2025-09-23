using Kommunist.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kommunist.Core.Converters;

public class ImageDetailsConverter : JsonConverter<ImageDetails>
{
    public override ImageDetails? ReadJson(JsonReader reader, Type objectType, ImageDetails? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        return reader.TokenType switch
        {
            JsonToken.String => new ImageDetails { Url = reader.Value?.ToString() ?? string.Empty },
            JsonToken.StartObject => DeserializeObject(reader, serializer),
            _ => ConsumeAndReturnNull(reader)
        };
    }

    private static ImageDetails DeserializeObject(JsonReader reader, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var result = new ImageDetails();
        using var jReader = jObject.CreateReader();
        serializer.Populate(jReader, result);
        return result;
    }

    private static ImageDetails? ConsumeAndReturnNull(JsonReader reader)
    {
        JToken.ReadFrom(reader);
        return null;
    }

    public override void WriteJson(JsonWriter writer, ImageDetails? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("url");
        writer.WriteValue(value.Url);
        writer.WriteEndObject();
    }
}