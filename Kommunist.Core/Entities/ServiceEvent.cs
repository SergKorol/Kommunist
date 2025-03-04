using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities;

public record ServiceEvent
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("event_url")]
    public string EventUrl { get; set; }

    [JsonProperty("registration_status_id")]
    public object RegistrationStatusId { get; set; }

    [JsonProperty("in_calendar")]
    public bool InCalendar { get; set; }

    [JsonProperty("types")]
    public List<string> Types { get; set; }

    [JsonProperty("talks")]
    public List<object> Talks { get; set; }

    [JsonProperty("speakers")]
    public List<Person> Speakers { get; set; }

    [JsonProperty("start")]
    public long Start { get; set; }

    [JsonProperty("end")]
    public long End { get; set; }

    [JsonProperty("language")]
    public string Language { get; set; }

    [JsonProperty("languages")]
    public List<string> Languages { get; set; }

    [JsonProperty("participation_format")]
    public ParticipationFormat ParticipationFormat { get; set; }

    [JsonProperty("is_past")]
    public bool IsPast { get; set; }
}