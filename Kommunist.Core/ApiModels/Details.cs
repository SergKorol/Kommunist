using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record Details
{
    [JsonProperty("is_past")]
    public bool IsPast { get; set; }

    [JsonProperty("dates_timestamp")]
    public DatesTimestamp? DatesTimestamp { get; set; }

    [JsonProperty("participation_format")]
    public ParticipationFormat? ParticipationFormat { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }
}