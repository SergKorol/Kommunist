using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record MenuItem
{
    [JsonProperty("key")]
    public string? Key { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("displayed_in")]
    public List<int>? DisplayedIn { get; set; }

    [JsonProperty("selected")]
    public bool Selected { get; set; }
}