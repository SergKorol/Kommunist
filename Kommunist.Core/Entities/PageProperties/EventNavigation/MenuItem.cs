using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.EventNavigation;

public record MenuItem
{
    [JsonProperty("key")]
    public string Key { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("displayed_in")]
    public int[] DisplayedIn { get; set; }
    [JsonProperty("selected")]
    public bool Selected { get; set; }
}