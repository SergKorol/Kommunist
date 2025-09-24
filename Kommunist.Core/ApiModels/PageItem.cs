using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record PageItem
{
    [JsonProperty("type")]
    public required string Type { get; set; }

    [JsonProperty("properties")]
    public required Properties Properties { get; set; }
}