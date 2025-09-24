namespace Kommunist.Core.ApiModels;

public class FilterOptions
{
    public string? TagFilter { get; init; }
    public List<string> TagFilters { get; init; } = [];
    public string? SpeakerFilter { get; init; }
    public List<string> SpeakerFilters { get; init; } = [];
    public List<string> CountryFilters { get; init; } = [];
    public string? CommunityFilter { get; init; }
    public List<string> CommunityFilters { get; init; } = [];
    public bool OnlineOnly { get; init; }
}