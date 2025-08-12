using Kommunist.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kommunist.Core.Converters;

public class TextItemListConverter : JsonConverter<List<TextItem>>
{
    public override List<TextItem> ReadJson(JsonReader reader, Type objectType, List<TextItem> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader is null) throw new ArgumentNullException(nameof(reader));

        switch (reader.TokenType)
        {
            case JsonToken.Null:
                return [];

            case JsonToken.String:
                return [new TextItem { Text = reader.Value?.ToString() ?? string.Empty }];

            case JsonToken.StartObject:
            {
                var obj = JObject.Load(reader);
                var item = obj.ToObject<TextItem>(serializer);
                return item != null ? [item] : new List<TextItem>();
            }

            case JsonToken.StartArray:
            {
                var array = JArray.Load(reader);
                var result = new List<TextItem>(array.Count);
                foreach (var token in array)
                {
                    if (token.Type == JTokenType.String)
                    {
                        result.Add(new TextItem { Text = token.Value<string>() ?? string.Empty });
                    }
                    else if (token.Type == JTokenType.Object)
                    {
                        var item = token.ToObject<TextItem>(serializer);
                        if (item != null)
                        {
                            result.Add(item);
                        }
                    }
                }

                return result;
            }

            case JsonToken.None:
            default:
            {
                var token = JToken.Load(reader);

                switch (token.Type)
                {
                    case JTokenType.Null:
                        return [];
                    case JTokenType.String:
                        return [new TextItem { Text = token.Value<string>() ?? string.Empty }];
                    case JTokenType.Object:
                    {
                        var item = token.ToObject<TextItem>(serializer);
                        return item != null ? [item] : [];
                    }
                    case JTokenType.Array:
                    {
                        var array = (JArray)token;
                        var result = new List<TextItem>(array.Count);
                        foreach (var t in array)
                        {
                            switch (t.Type)
                            {
                                case JTokenType.String:
                                    result.Add(new TextItem { Text = t.Value<string>() ?? string.Empty });
                                    break;
                                case JTokenType.Object:
                                {
                                    var item = t.ToObject<TextItem>(serializer);
                                    if (item != null)
                                    {
                                        result.Add(item);
                                    }

                                    break;
                                }
                            }
                        }

                        return result;
                    }
                    default:
                        throw new JsonSerializationException($"Unexpected token {token.Type} when parsing TextItem list at path '{reader.Path}'.");
                }
            }
        }
    }

    public override void WriteJson(JsonWriter writer, List<TextItem> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
