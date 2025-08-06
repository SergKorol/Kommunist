using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Agenda;

public record Navigation
{
    [JsonProperty("days")]
    public IEnumerable<Day> Days { get; set; }
    [JsonProperty("tracks")]
    public IEnumerable<Track> Tracks { get; set; }
}