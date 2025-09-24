using Kommunist.Core.ApiModels.BaseType;
using Newtonsoft.Json;
using Location = Kommunist.Core.ApiModels.PageProperties.Venue.Location;

namespace Kommunist.Core.ApiModels.PageProperties.Venue;

public record VenueProperties : IProperties
{
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
    [JsonProperty("locations")]
    public IEnumerable<Location>? Locations { get; set; }
    [JsonProperty("show_button")]
    public bool ShowButton { get; set; }
    [JsonProperty("google_maps_api_key")]
    public string? GoogleMapsApiKey { get; set; }
}