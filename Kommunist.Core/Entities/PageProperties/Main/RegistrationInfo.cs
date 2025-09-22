using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record RegistrationInfo
{
    [JsonProperty("button_type")]
    public string? ButtonType { get; set; }
    [JsonProperty("show_qr")] 
    public bool? ShowQr { get; set; }
    [JsonProperty("is_editable")]
    public bool? IsEditable { get; set; }
}