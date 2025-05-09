namespace Kommunist.Application.Models;

public class FilterOptions
{
    public string TagFilter { get; set; }
    public string SpeakerFilter { get; set; }
    public string CountryFilter { get; set; }
    public string CommunityFilter { get; set; }
    public bool OnlineOnly { get; set; }
}