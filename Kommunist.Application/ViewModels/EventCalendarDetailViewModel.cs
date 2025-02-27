using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Services.Interfaces;
using Markdig;
using Markdig.Renderers;
using Event = Kommunist.Core.Entities.Event;
using Item = Kommunist.Core.Models.Item;

namespace Kommunist.Application.ViewModels;

public class EventCalendarDetailViewModel
{
    public CalEventDetail SelectedEventDetail { get; set; }
    public Event TappedEvent { get; set; }
    public IEnumerable<EventPage> EventPages { get; set; }
    public IEnumerable<Item> Items { get; set; }
    public AgendaPage AgendaPage { get; set; }
    

    public int TappedEventId { get; set; }
    
    private readonly IEventService _eventService;

    public EventCalendarDetailViewModel(IEventService eventService, int tappedEventId)
    {
        _eventService = eventService;
        TappedEventId = tappedEventId;

        CreateEventCalendarDetailPage().ConfigureAwait(false);
    }

    private async Task CreateEventCalendarDetailPage()
    {
        // GetEventHome(TappedEventId).ConfigureAwait(false);
        await GetAgenda(TappedEventId);
        var mainPart = AgendaPage?.Agenda?.Items?.FirstOrDefault();
        if (mainPart == null)
        {
            await GetHomePages(TappedEventId);
            SetCalEventDetail(Items);
        }
        var agenda = AgendaPage.Agenda;
        
        var eventDetail = new CalEventDetail
        {
            EventId = TappedEventId,
            AgendaId = mainPart.Id,
            Title = mainPart.Title,
            BgImageUrl = mainPart.Image,
            PeriodDateTime = GetEventPeriod(agenda.Navigation.Days.First().Date, agenda.Navigation.Days.First().EndDate),
            // Url = $"https://wearecommunity.io/{navigationProperties.Event.EventUrl}",
            Language = mainPart.Language,
            FormatEvent = mainPart.IsOnline ? "Online" : "Offline",
            Location = mainPart.Location,
            // Description = $"&lt;HTML&gt;&lt;BODY&gt;{mainPart.Info.DescriptionHtml}&lt;/BODY&gt;&lt;HTML&gt;"
            Description = $"<![CDATA[<HTML><BODY>{mainPart.Info.DescriptionHtml}</BODY></HTML>]]>"
        };
        
        foreach (var speaker in mainPart.Speakers)
        {
            var speakerDetail = new PersonCard
            {
                SpeakerId = speaker.Id,
                Name = speaker.Name,
                Company = speaker.Company,
                Position = speaker.JobPosition,
                Avatar = speaker.AvatarSmall
        
            };
            eventDetail.Speakers.Add(speakerDetail);
        }

        foreach (var moderator in mainPart.Moderators)
        {
            var person = new PersonCard
            {
                SpeakerId = moderator.Id,
                Name = moderator.Name,
                Company = moderator.Company,
                Position = moderator.JobPosition,
                Avatar = moderator.AvatarSmall
        
            };
            eventDetail.Moderators.Add(person);
        }
        
        SelectedEventDetail = eventDetail;
    }

    private async Task GetAgenda(int eventId)
    {
        if (eventId != 0)
        {
            AgendaPage= await _eventService.GetAgenda(eventId);
        }
        
    }
    
    private async Task GetHomePages(int eventId)
    {
        if (eventId != 0)
        {
            Items = await _eventService.GetHomePage(eventId);
        }
        
    }

    private string GetEventPeriod(long start, long end)
    {
        var startDate = start.ToLocalDateTime();
        var endDate = end.ToLocalDateTime();
        if (startDate.Day == endDate.Day)
        {
            return  startDate.ToString("d MMM yyyy, HH:mm") + "-" + endDate.ToString("HH:mm");
        }

        return startDate.ToString("d MMM yyyy, HH:mm") + endDate.ToString("d MMM yyyy, HH:mm");
    }

    void SetCalEventDetail(IEnumerable<Item> items)
    {
        foreach(var item in items)
        {
            
        }
        
        // var eventDetail = new CalEventDetail
        // {
        //     EventId = TappedEventId,
        //     AgendaId = page.,
        //     Title = mainPart.Title,
        //     BgImageUrl = mainPart.Image,
        //     PeriodDateTime = GetEventPeriod(agenda.Navigation.Days.First().Date, agenda.Navigation.Days.First().EndDate),
        //     // Url = $"https://wearecommunity.io/{navigationProperties.Event.EventUrl}",
        //     Language = mainPart.Language,
        //     FormatEvent = mainPart.IsOnline ? "Online" : "Offline",
        //     Location = mainPart.Location,
        //     // Description = $"&lt;HTML&gt;&lt;BODY&gt;{mainPart.Info.DescriptionHtml}&lt;/BODY&gt;&lt;HTML&gt;"
        //     Description = $"<![CDATA[<HTML><BODY>{mainPart.Info.DescriptionHtml}</BODY></HTML>]]>"
        // };
        //
        // foreach (var speaker in mainPart.Speakers)
        // {
        //     var speakerDetail = new PersonCard
        //     {
        //         SpeakerId = speaker.Id,
        //         Name = speaker.Name,
        //         Company = speaker.Company,
        //         Position = speaker.JobPosition,
        //         Avatar = speaker.AvatarSmall
        //
        //     };
        //     eventDetail.Speakers.Add(speakerDetail);
        // }
        //
        // foreach (var moderator in mainPart.Moderators)
        // {
        //     var person = new PersonCard
        //     {
        //         SpeakerId = moderator.Id,
        //         Name = moderator.Name,
        //         Company = moderator.Company,
        //         Position = moderator.JobPosition,
        //         Avatar = moderator.AvatarSmall
        //
        //     };
        //     eventDetail.Moderators.Add(person);
        // }
        //
        // SelectedEventDetail = eventDetail;
    }
    
    // public string ConvertHtmlToMarkdown(string htmlContent)
    // {
    //     var pipeline = new MarkdownPipelineBuilder().Build();
    //     var markdownContent = Markdig.MarkdownExtensions;
    //     return markdownContent;
    // }
}