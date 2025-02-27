using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record IconText
{
    [JsonProperty("main")]
    public string Main { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}