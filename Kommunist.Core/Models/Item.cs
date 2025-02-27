using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Item
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("properties")]
    public Properties Properties { get; set; }
}