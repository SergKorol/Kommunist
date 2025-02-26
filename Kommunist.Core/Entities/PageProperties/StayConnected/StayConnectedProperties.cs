using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.PageProperties.EventNavigation;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.StayConnected;

public record StayConnectedProperties : IProperties
{
    [JsonProperty("bg_color")]
    public string BgColor { get; set; }
    [JsonProperty("opacity")]
    public string Opacity { get; set; }
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
    [JsonProperty("on_the_web")]
    public IEnumerable<OnTheWeb> OnTheWeb { get; set; }
    
}