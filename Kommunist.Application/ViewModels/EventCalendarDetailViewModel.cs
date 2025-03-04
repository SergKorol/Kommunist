using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Markdig;
using Markdig.Renderers;

namespace Kommunist.Application.ViewModels;

public class EventCalendarDetailViewModel : BaseViewModel
{
    public CalEventDetail SelectedEventDetail { get; set; }
    public ServiceEvent TappedServiceEvent { get; set; }
    public IEnumerable<EventPage> EventPages { get; set; }
    public IEnumerable<PageItem> PageItems { get; set; }
    public AgendaPage AgendaPage { get; set; }
    

    public int TappedEventId { get; set; }
    
    private readonly IEventService _eventService;

    public EventCalendarDetailViewModel(IEventService eventService, int tappedEventId)
    {
        _eventService = eventService;
        TappedEventId = tappedEventId;

        CreateEventCalendarDetailPage().ConfigureAwait(false);
    }
    
    private double _webViewHeight = 100; // Default height
    public double WebViewHeight
    {
        get => _webViewHeight;
        set
        {
            if (_webViewHeight != value)
            {
                _webViewHeight = value;
                OnPropertyChanged();
            }
        }
    }

    
    public string DescriptionWithNoScroll
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SelectedEventDetail?.Description))
                return string.Empty;

            return $@"
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        overflow: hidden; /* Hides scrolling */
                    }}
                </style>
            </head>
            <body>
                {SelectedEventDetail.Description}
            </body>
            </html>";
        }
    }

    private async Task CreateEventCalendarDetailPage()
    {
        await GetHomePage(TappedEventId);
        var eventDetail = new CalEventDetail();
        SetMainCalEventDetail(eventDetail);
        await SetAgendaPage(eventDetail);
        
        SelectedEventDetail = eventDetail;
    }

    private async Task GetAgenda(int eventId)
    {
        if (eventId != 0)
        {
            AgendaPage= await _eventService.GetAgenda(eventId);
        }
        
    }
    
    private async Task GetHomePage(int eventId)
    {
        if (eventId != 0)
        {
            PageItems = await _eventService.GetHomePage(eventId);
        }
        
    }

    private string GetEventPeriod(long? start, long? end)
    {
        if (start != null && end != null)
        {
            var startDate = start.Value.ToLocalDateTime();
            var endDate = end.Value.ToLocalDateTime();
            if (startDate.Day == endDate.Day)
            {
                return  startDate.ToString("d MMM yyyy, HH:mm") + "-" + endDate.ToString("HH:mm");
            }

            return startDate.ToString("d MMM yyyy, HH:mm") + " - " + endDate.ToString("d MMM yyyy, HH:mm");
        }
        return string.Empty;
    }

    void SetMainCalEventDetail(CalEventDetail eventDetail)
    {
        var mainPart = PageItems.FirstOrDefault(x => x.Type == "Main");
        if (mainPart?.Properties == null) return;
        eventDetail.EventId = TappedEventId;
        eventDetail.Title = mainPart.Properties?.Text?.First()?.Text;
        eventDetail.BgImageUrl = mainPart.Properties?.Image.Url;
        eventDetail.PeriodDateTime = GetEventPeriod(mainPart.Properties?.Details.DatesTimestamp.Start, mainPart.Properties?.Details.DatesTimestamp.End);
        eventDetail.Url = mainPart.Properties?.EventUrl;
        if (mainPart.Properties != null && mainPart.Properties.Languages.Any())
            eventDetail.Language = string.Join(", ", mainPart.Properties.Languages);
        eventDetail.FormatEvent = mainPart.Properties!.Details.ParticipationFormat.Online ? "Online" : "Offline";
        eventDetail.Location = mainPart.Properties.Details.ParticipationFormat.Location ?? "World";
        
        var iconPointsPart = PageItems.FirstOrDefault(x => x.Type == "IconPoints");
        if (iconPointsPart != null)
        {
            var texts = iconPointsPart.Properties.Text.Select(x => $"<p>{x.Text}</p>");
            eventDetail.Description = $"<![CDATA[<HTML><BODY>{string.Join("\n", texts)}</BODY></HTML>";
        }
    }

    async Task SetAgendaPage(CalEventDetail eventDetail)
    {
        var agendaPart = PageItems.FirstOrDefault(x => x.Type == "Agenda");
        if (agendaPart?.Properties == null) return;
        await GetAgenda(TappedEventId);
        if (AgendaPage.Agenda.Items.Any())
        {
            var agendaItem = AgendaPage.Agenda.Items.First();
            if (agendaItem != null && agendaItem.Speakers?.Any() == true)
            {
                foreach (var speaker in agendaItem.Speakers)
                {
                    var speakerCard = new PersonCard
                    {
                        SpeakerId = speaker.Id,
                        Name = speaker.Name,
                        Company = speaker.Company,
                        Position = speaker.JobPosition,
                        Avatar = speaker.AvatarSmall
                    };
                    eventDetail.Speakers.Add(speakerCard);
                }
            }

            if (agendaItem != null && agendaItem.Moderators?.Any() == true)
            {
                foreach (var moderator in agendaItem.Moderators)
                {
                    var moderatorCard = new PersonCard
                    {
                        SpeakerId = moderator.Id,
                        Name = moderator.Name,
                        Company = moderator.Company,
                        Position = moderator.JobPosition,
                        Avatar = moderator.AvatarSmall
                    };
                    eventDetail.Moderators.Add(moderatorCard);
                }
            }

            if (agendaItem?.Info?.DescriptionHtml != null)
            {
                eventDetail.Description = $@"
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        
                    }}
                </style>
            </head>
            <body>
                {agendaItem.Info.DescriptionHtml}
            </body>
            </html>";
            }
        }
    }

    // public string ConvertHtmlToMarkdown(string htmlContent)
    // {
    //     var pipeline = new MarkdownPipelineBuilder().Build();
    //     var markdownContent = Markdig.MarkdownExtensions;
    //     return markdownContent;
    // }
}