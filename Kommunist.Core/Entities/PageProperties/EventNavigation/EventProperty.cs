using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.EventNavigation;

public record EventProperty
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("event_url")]
    public string? EventUrl { get; set; }
    [JsonProperty("title")]
    public string? Title { get; set; }
    [JsonProperty("is_multiple_page")]
    public bool IsMultiplePage { get; set; }
    [JsonProperty("entity_locale")]
    public string? EntityLocale { get; set; }
    [JsonProperty("menu_items")]
    public IEnumerable<MenuItem>? MenuItems { get; set; }
    [JsonProperty("copyright")]
    public string? Copyright { get; set; }
    [JsonProperty("org_team")]
    public string? OrgTeam { get; set; }
    [JsonProperty("on_the_web")]
    public IEnumerable<OnTheWeb>? OnTheWeb { get; set; }
    [JsonProperty("can_manage")]
    public string? CanManage { get; set; }
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("is_past")]
    public bool IsPast { get; set; }
    [JsonProperty("attendee_registration_type_id")]
    public int AttendeeRegistrationTypeId { get; set; }
    [JsonProperty("logo")]
    public Logo? Logo { get; set; }
    [JsonProperty("color")]
    public string? Color { get; set; }
}