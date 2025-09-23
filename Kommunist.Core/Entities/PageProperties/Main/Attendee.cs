using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record Attendee
{
    
    [JsonProperty("open")]
    public bool Open { get; set; }
    [JsonProperty("limit_info")]
    public LimitInfo? LimitInfo { get; set; }
    [JsonProperty("registration_info")]
    public RegistrationInfo? RegistrationInfo { get; set; }
    [JsonProperty("registration_status_id")]
    public string? RegistrationStatusId { get; set; }
    [JsonProperty("is_global")]
    public bool IsGlobal { get; set; }
}