using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record BgImage
{
    [JsonProperty("normal")]
    public string Normal { get; set; }
}