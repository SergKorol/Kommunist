using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Details
{
    [JsonProperty("is_past")]
    public bool IsPast { get; set; }

    [JsonProperty("dates_timestamp")]
    public DatesTimestamp DatesTimestamp { get; set; }

    [JsonProperty("participation_format")]
    public ParticipationFormat ParticipationFormat { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}