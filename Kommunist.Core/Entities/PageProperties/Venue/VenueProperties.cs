using System.Collections.Generic;
using Kommunist.Core.Entities.BaseType;
using Newtonsoft.Json;

namespace Kommunist.Core.Entities.PageProperties.Venue;

public record VenueProperties : IProperties
{
    [JsonProperty("show_on_mobile_app")]
    public bool ShowOnMobileApp { get; set; }
    [JsonProperty("locations")]
    public IEnumerable<Location> Locations { get; set; }
    [JsonProperty("show_button")]
    public bool ShowButton { get; set; }
    [JsonProperty("google_maps_api_key")]
    public string GoogleMapsApiKey { get; set; }
}