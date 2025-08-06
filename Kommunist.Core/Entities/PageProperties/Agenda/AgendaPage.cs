using Newtonsoft.Json;
using TimeZone = Kommunist.Core.Entities.PageProperties.Main.TimeZone;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record AgendaPage
{
    [JsonProperty("agenda")]
    public AgendaDetail Agenda { get; set; }
    [JsonProperty("custom_fields")]
    public string[] CustomFields { get; set; }
    [JsonProperty("is_registration_open")]
    public bool IsRegistrationOpen { get; set; }
    [JsonProperty("show_global")]
    public bool ShowGlobal { get; set; }
    [JsonProperty("nearest_locations")]
    public string[] NearestLocations { get; set; }
    [JsonProperty("notify_not_logged")]
    public bool NotifyNotLogged { get; set; }
    [JsonProperty("time_zones")]
    public IEnumerable<TimeZone> TimeZones { get; set; }
    [JsonProperty("event_is_past")]
    public bool EventIsPast { get; set; }
    [JsonProperty("recommended_talks")] 
    public string[] RecommendedTalks { get; set; }
    [JsonProperty("user_primary_skill")]
    public string UserPrimarySkill { get; set; }
}