using Newtonsoft.Json;

namespace Kommunist.Core.ApiModels.PageProperties.Main;

public record LimitInfo
{
    [JsonProperty("is_reached")]
    public bool IsReached { get; set; }
    [JsonProperty("hybrid_is_reached")]
    public bool HybridIsReached { get; set; }
    [JsonProperty("status")]
    public string? Status { get; set; }
    [JsonProperty("free_discount")]
    public bool FreeDiscount { get; set; }
}