using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Agenda;

public record AgendaDetail
{
    [JsonProperty("items")]
    public IEnumerable<Item>? Items { get; set; }
    [JsonProperty("navigation")]
    public Navigation? Navigation { get; set; }
}