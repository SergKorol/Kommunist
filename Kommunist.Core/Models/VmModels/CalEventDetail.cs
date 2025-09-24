namespace Kommunist.Core.Models.VmModels;

public class CalEventDetail
{
    public string? Title { get; set; }
    public string? BgImageUrl { get; set; }
    public string? PeriodDateTime { get; set; }
    public string? Date { get; set; }
    public string? Url { get; set; }
    public string? Language { get; set; }
    public string? FormatEvent { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public List<PersonCard> Speakers { get; set; } = [];
    public List<PersonCard> Moderators { get; set; } = [];
}

public class PersonCard
{
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }
}