using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels;

public record Event
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("event_url")]
    public string? EventUrl { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("menu_items")]
    public List<MenuItem>? MenuItems { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }
}