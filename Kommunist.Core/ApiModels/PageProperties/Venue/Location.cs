using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Venue;

public record Location
{
    [JsonProperty("title")]
    public string? Title { get; set; }
    [JsonProperty("lat")]
    public string? Latitude { get; set; }
    [JsonProperty("lon")]
    public string? Longitude { get; set; }
}