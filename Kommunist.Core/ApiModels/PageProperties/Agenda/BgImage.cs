using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Agenda;

public record BgImage
{
    [JsonProperty("normal")]
    public string? Normal { get; set; }
}