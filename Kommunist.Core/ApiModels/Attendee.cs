using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Attendee
{
    [JsonProperty("open")]
    public bool Open { get; set; }
}