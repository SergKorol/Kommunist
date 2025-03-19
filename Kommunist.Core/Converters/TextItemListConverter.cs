using Kommunist.Core.Models;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kommunist.Core.Converters;

public class TextItemListConverter : JsonConverter<List<TextItem>>
{
    public override List<TextItem> ReadJson(JsonReader reader, Type objectType, List<TextItem> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new List<TextItem>();

        if (reader.TokenType == JsonToken.String)
        {
            result.Add(new TextItem { Text = reader.Value.ToString() });
        }
        else if (reader.TokenType == JsonToken.StartArray)
        {
            result = serializer.Deserialize<List<TextItem>>(reader);
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, List<TextItem> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
