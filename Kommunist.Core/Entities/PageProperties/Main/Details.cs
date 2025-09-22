using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record Details
{
    [JsonProperty("is_past")]
    public bool IsPast { get; set; }
    [JsonProperty("dates")]
    public string? Dates { get; set; }
    [JsonProperty("dates_timestamp")]
    public DatesTimestamp? DatesTimestamp { get; set; }
    [JsonProperty("default_time_format")]
    public bool DefaultTimeFormat { get; set; }
    [JsonProperty("participation_format")]
    public ParticipationFormat? ParticipationFormat { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("timezones")]
    public IEnumerable<TimeZone>? TimeZones { get; set; }
    [JsonProperty("internal")]
    public bool Internal { get; set; }
}