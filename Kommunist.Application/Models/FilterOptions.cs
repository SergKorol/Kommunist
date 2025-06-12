namespace Kommunist.Application.Models;

public class FilterOptions
{
    public string TagFilter { get; set; }
    public string SpeakerFilter { get; set; }
    public List<string> CountryFilters { get; set; } = new List<string>(); // Changed from single CountryFilter to list
    public string CommunityFilter { get; set; }
    public bool OnlineOnly { get; set; }
}