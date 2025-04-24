using System.Collections.Generic;

namespace Kommunist.Application.Models;

public record CalEventDetail
{
    public int EventId { get; set; }
    public int AgendaId { get; set; }
    public string Title { get; set; }
    public string BgImageUrl { get; set; }
    public string PeriodDateTime { get; set; }
    public string Date { get; set; }
    public string Url { get; set; }
    public string Language { get; set; }
    public string FormatEvent { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public List<PersonCard> Speakers { get; set; } = new List<PersonCard>();
    public List<PersonCard> Moderators { get; set; } = new List<PersonCard>();
}

public record PersonCard
{
    public string SpeakerId { get; set; }
    public string Name { get; set; }
    public string? Avatar { get; set; }
    public string Company { get; set; }
    public string Position { get; set; }
}