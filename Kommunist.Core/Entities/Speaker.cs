using Newtonsoft.Json;

namespace Kommunist.Core.Entities;

public record Speaker
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("with_profile")]
    public bool WithProfile { get; set; }

    [JsonProperty("avatar")]
    public string Avatar { get; set; }

    [JsonProperty("avatar_small")]
    public string AvatarSmall { get; set; }

    [JsonProperty("is_deleted")]
    public object IsDeleted { get; set; }

    [JsonProperty("company")]
    public string Company { get; set; }

    [JsonProperty("job_position")]
    public string JobPosition { get; set; }

    [JsonProperty("company_and_title")]
    public string CompanyAndTitle { get; set; }

    [JsonProperty("slug")]
    public string Slug { get; set; }
}