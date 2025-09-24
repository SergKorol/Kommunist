using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Main;

public record WatchRecording
{
    [JsonProperty("show")]
    public bool Show { get; set; }
}