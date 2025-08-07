using Kommunist.Core.Models;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kommunist.Core.Converters;

public class TextItemListConverter : JsonConverter<List<TextItem>>
{
    public override List<TextItem> ReadJson(JsonReader reader, Type objectType, List<TextItem> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new List<TextItem>();

        switch (reader.TokenType)
        {
            case JsonToken.String:
                result.Add(new TextItem { Text = reader.Value?.ToString() ?? "" });
                break;
            case JsonToken.StartArray:
                result = serializer.Deserialize<List<TextItem>>(reader);
                break;
            case JsonToken.None:
            case JsonToken.StartObject:
            case JsonToken.StartConstructor:
            case JsonToken.PropertyName:
            case JsonToken.Comment:
            case JsonToken.Raw:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Boolean:
            case JsonToken.Null:
            case JsonToken.Undefined:
            case JsonToken.EndObject:
            case JsonToken.EndArray:
            case JsonToken.EndConstructor:
            case JsonToken.Date:
            case JsonToken.Bytes:
            default:
                throw new ArgumentOutOfRangeException(nameof(reader));
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, List<TextItem> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
