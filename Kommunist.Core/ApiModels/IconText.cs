using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record IconText
{
    [JsonProperty("main")]
    public string? Main { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }
}