using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record ParticipationFormat
{
    [JsonProperty("online")]
    public bool Online { get; set; }
    
    [JsonProperty("location")]
    public string? Location { get; set; }
}