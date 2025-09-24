using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Agenda;

public record Day
{
    [JsonProperty("id")]
    public short Id { get; set; }
    [JsonProperty("date")]
    public long Date { get; set; }
    [JsonProperty("end_date")]
    public long EndDate { get; set; }
    [JsonProperty("date_by_day")]
    public long DateByDay { get; set; }
}