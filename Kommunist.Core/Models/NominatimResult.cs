using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public class NominatimResult
{
    [JsonProperty("lat")]
    public string Lat { get; set; }
    [JsonProperty("lon")]
    public string Lon { get; set; }
}