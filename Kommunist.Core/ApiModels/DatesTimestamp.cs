using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record DatesTimestamp
{
    [JsonProperty("start")]
    public long Start { get; set; }

    [JsonProperty("end")]
    public long End { get; set; }
}