using Kommunist.Core.ApiModels.BaseType;
using Kommunist.Core.ApiModels.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.UnlimitedText;

public record UnlimitedTextProperties : IProperties
{
    [JsonProperty("bg_color")]
    public string? BgColor { get; set; }
    [JsonProperty("opacity")]
    public string? Opacity { get; set; }
    [JsonProperty("text")]
    public IEnumerable<TextProperty>? Text { get; set; }
    [JsonProperty("unlimitedText")]
    public string? UnlimitedText { get; set; }
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
}