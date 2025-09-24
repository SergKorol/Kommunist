using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record Attendee
{
    [JsonProperty("open")]
    public bool Open { get; set; }
}