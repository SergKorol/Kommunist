using Kommunist.Core.ApiModels.BaseType;
using Kommunist.Core.ApiModels.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Agenda;

public record AgendaProperties : IProperties
{
    [JsonProperty("quick_filters_color")]
    public string? QuickFiltersColor { get; set; }
    [JsonProperty("text")]
    public IEnumerable<TextProperty>? Text { get; set; }
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
    [JsonProperty("image")]
    public string? Image { get; set; }
    [JsonProperty("is_block")]
    public bool IsBlock { get; set; }
    [JsonProperty("show_all")]
    public bool ShowAll { get; set; }
    [JsonProperty("url")]
    public string? Url { get; set; }
}