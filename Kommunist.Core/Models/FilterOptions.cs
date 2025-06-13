namespace Kommunist.Core.Models;

public class FilterOptions
{
    public string TagFilter { get; set; }
    public List<string> TagFilters { get; set; } = new();
    public string SpeakerFilter { get; set; }
    public List<string> SpeakerFilters { get; set; } = new ();
    public List<string> CountryFilters { get; set; } = new();
    public string CommunityFilter { get; set; }
    public List<string> CommunityFilters { get; set; } = new();
    public bool OnlineOnly { get; set; }
}