using Kommunist.Core.Converters;
using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.Enums;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities;

[JsonConverter(typeof(PropertiesConverter))]
public record EventPage
{
    [JsonProperty("type")]
    public PageType Type { get; set; }
    [JsonProperty("properties")]
    public IProperties? Properties { get; set; }
}