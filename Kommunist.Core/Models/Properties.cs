using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Main;
using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public record Properties
{
    [JsonProperty("event")]
    public Event Event { get; set; }

    [JsonProperty("community")]
    public object Community { get; set; }

    [JsonProperty("bg_color")]
    public string BgColor { get; set; }

    [JsonProperty("opacity")]
    public string Opacity { get; set; }

    [JsonProperty("without_details")]
    public bool? WithoutDetails { get; set; }

    [JsonProperty("text")]
    public List<TextItem> Text { get; set; }

    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("details")]
    public Details Details { get; set; }

    [JsonProperty("is_main_page")]
    public bool? IsMainPage { get; set; }

    [JsonProperty("registration")]
    public Registration Registration { get; set; }

    [JsonProperty("default_icon_color")]
    public string DefaultIconColor { get; set; }

    [JsonProperty("icons")]
    public List<Icon> Icons { get; set; }
}