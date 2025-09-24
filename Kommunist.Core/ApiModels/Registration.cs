using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record Registration
{
    [JsonProperty("attendee")]
    public Attendee? Attendee { get; set; }
}