using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Icon
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("bg_color")]
    public string BgColor { get; set; }

    [JsonProperty("text")]
    public IconText Text { get; set; }
}