using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record Level
{
    [JsonProperty("text")]
    public string Text { get; set; }
    [JsonProperty("icon")]
    public string Icon { get; set; }
}