using Kommunist.Core.Entities.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Registration
{
    [JsonProperty("attendee")]
    public Attendee Attendee { get; set; }
}