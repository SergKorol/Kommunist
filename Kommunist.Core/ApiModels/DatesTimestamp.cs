using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record DatesTimestamp
{
    [JsonProperty("start")]
    public long Start { get; set; }

    [JsonProperty("end")]
    public long End { get; set; }
}