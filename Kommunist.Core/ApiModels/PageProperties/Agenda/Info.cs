using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Agenda;

public record Info
{
    [JsonProperty("description")]
    public string? DescriptionHtml { get; set; }
    [JsonProperty("level")]
    public Level? Level { get; set; }
    [JsonProperty("skills")]
    public string[]? Tags { get; set; }
}