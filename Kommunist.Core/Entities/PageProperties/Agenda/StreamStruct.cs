using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record StreamStruct
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
}