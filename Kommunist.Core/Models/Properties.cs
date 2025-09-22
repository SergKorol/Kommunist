using Kommunist.Core.Converters;
using Newtonsoft.Json;

namespace Kommunist.Core.Models;

public class Properties
{
    [JsonProperty("event")]
    public Event? Event { get; set; }
    
    [JsonProperty("bg_color")]
    public string BackgroundColor { get; set; }

    [JsonProperty("opacity")]
    public string? Opacity { get; set; }

    [JsonProperty("without_details")]
    public bool WithoutDetails { get; set; }

    [JsonProperty("text")]
    [JsonConverter(typeof(TextItemListConverter))]
    public List<TextItem>? Text { get; set; }

    [JsonProperty("unlimitedText")]
    public string? UnlimitedText { get; set; }

    [JsonProperty("icons")]
    public IEnumerable<Icon>? Icons { get; set; }

    [JsonProperty("image")]
    [JsonConverter(typeof(ImageDetailsConverter))]
    public ImageDetails? Image { get; set; }

    [JsonProperty("details")]
    public Details? Details { get; set; }

    [JsonProperty("is_main_page")]
    public bool IsMainPage { get; set; }

    [JsonProperty("registration")]
    public Registration? Registration { get; set; }

    [JsonProperty("subscription")]
    public object? Subscription { get; set; }

    [JsonProperty("is_registration")]
    public object? IsRegistration { get; set; }

    [JsonProperty("is_speaker")]
    public object? IsSpeaker { get; set; }

    [JsonProperty("event_url")]
    public string? EventUrl { get; set; }

    [JsonProperty("communities")]
    public List<object>? Communities { get; set; }

    [JsonProperty("languages")]
    public List<string>? Languages { get; set; }

    [JsonProperty("show_qr")]
    public object? ShowQr { get; set; }

    [JsonProperty("show_qr_scanner")]
    public object? ShowQrScanner { get; set; }

    [JsonProperty("show_pinned_bar")]
    public bool ShowPinnedBar { get; set; }
}

public class ImageDetails
{
    [JsonProperty("url")]
    public string? Url { get; set; }
}