using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Main;

public record TextProperty
{
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("text")]
    public string? Text { get; set; }
    [JsonProperty("maxLength")]
    public string? Maxlength { get; set; }
}