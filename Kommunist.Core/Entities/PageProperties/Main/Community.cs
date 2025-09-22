using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record Community
{
    [JsonProperty("url")]
    public string? Url { get; set; }
    [JsonProperty("title")]
    public string? Title { get; set; }
    [JsonProperty("logo")]
    public string? Logo { get; set; }
}