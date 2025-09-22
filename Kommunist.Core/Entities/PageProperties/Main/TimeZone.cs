using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record TimeZone
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("zone_name")]
    public string? ZoneName { get; set; }
    [JsonProperty("maxLength")]
    public long Offset { get; set; }
}