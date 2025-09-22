using Kommunist.Core.Entities.BaseType;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record MainProperties : IProperties
{
    [JsonProperty("bg_color")]
    public string? BgColor { get; set; }
    [JsonProperty("opacity")]
    public string? Opacity { get; set; }
    [JsonProperty("text")]
    public IEnumerable<TextProperty>? Text { get; set; }
    [JsonProperty("without_details")]
    public bool WithoutDetails { get; set; }
    [JsonProperty("image")]
    public string? Image { get; set; }
    [JsonProperty("details")]
    public Details? Details { get; set; }
    [JsonProperty("is_main_page")]
    public bool IsMainPage { get; set; }
    [JsonProperty("registration")]
    public Registration? Registration { get; set; }
    [JsonProperty("subscription")]
    public string? Subscription { get; set; }
    [JsonProperty("is_registration")]
    public string? IsRegistration { get; set; }
    [JsonProperty("is_speaker")]
    public string? IsSpeaker { get; set; }
    [JsonProperty("event_url")]
    public string? EventUrl { get; set; }
    [JsonProperty("communities")]
    public IEnumerable<Community>? Communities { get; set; }
    [JsonProperty("languages")]
    public string[]? Languages { get; set; }
    [JsonProperty("show_qr")]
    public string? ShowQr { get; set; }
    [JsonProperty("show_qr_scanner")]
    public string? ShowQrScanner { get; set; }
    [JsonProperty("show_pinned_bar")]
    public string? ShowPinnedBar { get; set; }
}