using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.EventNavigation;

public record OnTheWeb
{
    [JsonProperty("icon")]
    public string Icon { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("link")]
    public string Link { get; set; }
}