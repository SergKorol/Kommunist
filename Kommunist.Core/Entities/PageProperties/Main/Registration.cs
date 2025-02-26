using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Main;

public record Registration
{
    [JsonProperty("attendee")]
    public Attendee Attendee { get; set; }
    [JsonProperty("reg_status")]
    public string RegStatus { get; set; }
    [JsonProperty("join_online")]
    public JoinOnline JoinOnline { get; set; }
    [JsonProperty("watch_recording")]
    public WatchRecording WatchRecording { get; set; }
}