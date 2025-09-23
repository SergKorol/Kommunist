using Newtonsoft.Json;

namespace Kommunist.Core.Entities;

public record ParticipationFormat
{
    [JsonProperty("online")]
    public bool Online { get; set; }
    
    [JsonProperty("online_label")]
    public string? OnlineLabel { get; set; }

    [JsonProperty("indoor")]
    public bool Indoor { get; set; }

    [JsonProperty("location")]
    public string? Location { get; set; }
}