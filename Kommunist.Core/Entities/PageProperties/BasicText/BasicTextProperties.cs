using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.BasicText;

public record BasicTextProperties : IProperties
{
    [JsonProperty("bg_color")]
    public string BgColor { get; set; }
    [JsonProperty("opacity")]
    public string Opacity { get; set; }
    [JsonProperty("text")]
    public IEnumerable<TextProperty> Text { get; set; }
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
}