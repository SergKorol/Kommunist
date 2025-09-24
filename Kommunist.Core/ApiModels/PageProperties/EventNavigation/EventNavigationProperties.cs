using Kommunist.Core.ApiModels.BaseType;
using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.EventNavigation;

public record EventNavigationProperties : IProperties
{
    [JsonProperty("event")]
    public EventProperty? Event { get; set; }
    [JsonProperty("community")]
    public string? Community { get; set; }
}