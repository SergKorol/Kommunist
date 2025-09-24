using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Main;

public record JoinOnline
{
    [JsonProperty("show")]
    public bool Show { get; set; }
    [JsonProperty("is_active")]
    public bool IsActive { get; set; }
}