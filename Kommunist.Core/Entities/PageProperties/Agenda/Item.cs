using System.Collections.Generic;
using Kommunist.Core.Entities.PageProperties.Main;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record Item
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("weight")]
    public short Weight { get; set; }
    [JsonProperty("day_id")]
    public short DayId { get; set; }
    [JsonProperty("track_id")]
    public int TrackId { get; set; }
    [JsonProperty("date")]
    public long Date { get; set; }
    [JsonProperty("is_speech")]
    public bool IsSpeech { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("duration")]
    public string Duration { get; set; }
    [JsonProperty("duration_min")]
    public int DurationMin { get; set; }
    [JsonProperty("color")]
    public string Color { get; set; }
    [JsonProperty("location")]
    public string Location { get; set; }
    [JsonProperty("room")]
    public string Room { get; set; }
    [JsonProperty("is_online")]
    public bool IsOnline { get; set; }
    [JsonProperty("filter_location")]
    public string FilterLocation { get; set; }
    [JsonProperty("is_online_broadcast")]
    public bool IsOnlineBroadcast { get; set; }
    [JsonProperty("skills")]
    public string[] Tags { get; set; }
    [JsonProperty("is_link_to_stream")]
    public string IsLinkToStream { get; set; }
    [JsonProperty("stream_struct")]
    public StreamStruct StreamStruct { get; set; }
    [JsonProperty("is_link_to_recording")]
    public bool IsLinkToRecording { get; set; }
    [JsonProperty("is_recommended")]
    public bool IsRecommended { get; set; }
    [JsonProperty("talk_type")]
    public string TalkType { get; set; }
    [JsonProperty("image")]
    public string Image { get; set; }
    [JsonProperty("talk_level")] 
    public string TalkLevel { get; set; }
    [JsonProperty("talk_level_id")] 
    public short TalkLevelId { get; set; }
    [JsonProperty("language")]
    public string Language { get; set; }
    [JsonProperty("short_language")]
    public string ShortLanguage { get; set; }
    [JsonProperty("speakers")]
    public IEnumerable<Person> Speakers { get; set; } = new List<Person>();
    [JsonProperty("moderators")]
    public IEnumerable<Person> Moderators { get; set; } = new List<Person>();
    [JsonProperty("background_image")]
    public string BackgroundImage { get; set; }
    [JsonProperty("info")]
    public Info Info { get; set; }
    [JsonProperty("is_speaker")]
    public bool IsSpeaker { get; set; }
    [JsonProperty("is_talk_moderator")]
    public bool IsTalkModerator { get; set; }
    [JsonProperty("in_calendar")]
    public bool InCalendar { get; set; }
    [JsonProperty("in_collection")]
    public bool InCollection { get; set; }
    [JsonProperty("can_add_to_collection")]
    public bool CanAddToCollection { get; set; }
    [JsonProperty("registration_info")]
    public RegistrationInfo RegistrationInfo { get; set; }
}