using Kommunist.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kommunist.Core.Converters;

public class TextItemListConverter : JsonConverter<List<TextItem>>
{
    public override List<TextItem> ReadJson(JsonReader reader, Type objectType, List<TextItem> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(reader);

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
                return item != null ? [item] : [];
            }

            case JsonToken.StartArray:
            {
                var array = JArray.Load(reader);
                var result = new List<TextItem>(array.Count);
                foreach (var token in array)
                {
                    switch (token.Type)
                    {
                        case JTokenType.String:
                            result.Add(new TextItem { Text = token.Value<string>() ?? string.Empty });
                            break;
                        case JTokenType.Object:
                        {
                            var item = token.ToObject<TextItem>(serializer);
                            if (item != null)
                            {
                                result.Add(item);
                            }

                            break;
                        }
                        case JTokenType.None:
                        case JTokenType.Array:
                        case JTokenType.Constructor:
                        case JTokenType.Property:
                        case JTokenType.Comment:
                        case JTokenType.Integer:
                        case JTokenType.Float:
                        case JTokenType.Boolean:
                        case JTokenType.Null:
                        case JTokenType.Undefined:
                        case JTokenType.Date:
                        case JTokenType.Raw:
                        case JTokenType.Bytes:
                        case JTokenType.Guid:
                        case JTokenType.Uri:
                        case JTokenType.TimeSpan:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(token.Type.ToString());
                    }
                }

                return result;
            }

            case JsonToken.None:
            case JsonToken.StartConstructor:
            case JsonToken.PropertyName:
            case JsonToken.Comment:
            case JsonToken.Raw:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.Boolean:
            case JsonToken.Undefined:
            case JsonToken.EndObject:
            case JsonToken.EndArray:
            case JsonToken.EndConstructor:
            case JsonToken.Date:
            case JsonToken.Bytes:
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
                                case JTokenType.None:
                                case JTokenType.Array:
                                case JTokenType.Constructor:
                                case JTokenType.Property:
                                case JTokenType.Comment:
                                case JTokenType.Integer:
                                case JTokenType.Float:
                                case JTokenType.Boolean:
                                case JTokenType.Null:
                                case JTokenType.Undefined:
                                case JTokenType.Date:
                                case JTokenType.Raw:
                                case JTokenType.Bytes:
                                case JTokenType.Guid:
                                case JTokenType.Uri:
                                case JTokenType.TimeSpan:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(t.Type.ToString());
                            }
                        }

                        return result;
                    }
                    case JTokenType.None:
                    case JTokenType.Constructor:
                    case JTokenType.Property:
                    case JTokenType.Comment:
                    case JTokenType.Integer:
                    case JTokenType.Float:
                    case JTokenType.Boolean:
                    case JTokenType.Undefined:
                    case JTokenType.Date:
                    case JTokenType.Raw:
                    case JTokenType.Bytes:
                    case JTokenType.Guid:
                    case JTokenType.Uri:
                    case JTokenType.TimeSpan:
                    default:
                        throw new JsonSerializationException($"Unexpected token {token.Type} when parsing TextItem list at path '{reader.Path}'.");
                }
            }
        }
    }

    public override void WriteJson(JsonWriter writer, List<TextItem> value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            serializer.Serialize(writer, item);
        }
        writer.WriteEndArray();
    }
}
