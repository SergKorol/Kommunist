using Kommunist.Core.ApiModels.BaseType;
using Kommunist.Core.ApiModels.Enums;
using Kommunist.Core.Converters;
using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

[JsonConverter(typeof(PropertiesConverter))]
public record EventPage
{
    [JsonProperty("type")]
    public PageType Type { get; set; }
    [JsonProperty("properties")]
    public IProperties? Properties { get; set; }
}