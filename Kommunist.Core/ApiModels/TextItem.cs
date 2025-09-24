using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record TextItem
{
    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("text")]
    public string? Text { get; set; }

    [JsonProperty("maxLength")]
    public int MaxLength { get; set; }
}