using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record Track
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("title")]
    public string? Title { get; set; }
    [JsonProperty("color")]
    public string? Color { get; set; }
    [JsonProperty("time_zone")]
    public string? TimeZone { get; set; }
    [JsonProperty("is_link_to_stream")]
    public string? IsLinkToStream { get; set; }
    [JsonProperty("is_link_to_recording")]
    public string? IsLinkToRecording { get; set; }
}