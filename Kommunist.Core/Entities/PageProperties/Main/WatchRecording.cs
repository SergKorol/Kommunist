using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record WatchRecording
{
    [JsonProperty("show")]
    public bool Show { get; set; }
}