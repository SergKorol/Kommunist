using System.Text;
using System.Windows.Input;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Attendee = Ical.Net.DataTypes.Attendee;

namespace Kommunist.Application.ViewModels;

public class EventCalendarDetailViewModel : BaseViewModel
{
    private readonly IFileHostingService _fileHostingService;
    
    public CalEventDetail SelectedEventDetail { get; set; }
    public ServiceEvent TappedServiceEvent { get; set; }
    public IEnumerable<EventPage> EventPages { get; set; }
    public IEnumerable<PageItem> PageItems { get; set; }
    public AgendaPage AgendaPage { get; set; }
    

    public int TappedEventId { get; set; }
    
    private readonly IEventService _eventService;
    
    public ICommand AddToCalendar { get; }
    public ICommand JoinToEvent { get; }

    public EventCalendarDetailViewModel(IEventService eventService, int tappedEventId, IFileHostingService fileHostingService)
    {
        _eventService = eventService;
        _fileHostingService = fileHostingService;
        TappedEventId = tappedEventId;

        CreateEventCalendarDetailPage().ConfigureAwait(false);
        AddToCalendar = new Command(GenerateEventAndUpload);
        JoinToEvent = new Command(OpenEventPage);
    }
    
    private async Task CreateEventCalendarDetailPage()
    {
        await GetHomePage(TappedEventId);
        var eventDetail = new CalEventDetail();
        SetMainCalEventDetail(eventDetail);
        await SetAgendaPage(eventDetail);
        
        SelectedEventDetail = eventDetail;
    }
    
    public bool HasParticipants => 
        SelectedEventDetail != null && 
        ((SelectedEventDetail.Speakers != null && SelectedEventDetail.Speakers.Count > 0) || 
         (SelectedEventDetail.Moderators != null && SelectedEventDetail.Moderators.Count > 0));

    private async Task GetAgenda(int eventId)
    {
        if (eventId != 0)
        {
            AgendaPage= await _eventService.GetAgenda(eventId);
        }
        
    }

    private async void OpenEventPage()
    {
        await Launcher.OpenAsync(SelectedEventDetail.Url.Trim());
    }

    private async void GenerateEventAndUpload()
    {
        var properties = PageItems.FirstOrDefault(x => x.Type == "Main")?.Properties;
        if (properties == null) return;
        
        var calendar = new Ical.Net.Calendar
        {
            Method = "PUBLISH",
            Scale = "GREGORIAN"
        };
        calendar.TimeZones.Add(new VTimeZone { TzId = TimeZoneInfo.Local.Id});
        
        var alarm = new Alarm
        {
            Trigger = new Ical.Net.DataTypes.Trigger("-PT5M"),
            Description = "Reminder",
            Action = AlarmAction.Display,
        };
        var datesStamp = properties?.Details.DatesTimestamp;
        var icalEvent = new CalendarEvent
        {
            Start = new CalDateTime(ConvertDateTime(datesStamp.Start), TimeZoneInfo.Local.Id),
            Summary = SelectedEventDetail.Title,
            Description = $"{HtmlConverter.HtmlToPlainText(SelectedEventDetail.Description)}" + "\n\n" + $"{SelectedEventDetail.Url}",
            DtStart = new CalDateTime(ConvertDateTime(datesStamp.Start), TimeZoneInfo.Local.Id),
            DtEnd = new CalDateTime(ConvertDateTime(datesStamp.End), TimeZoneInfo.Local.Id),
            Transparency = TransparencyType.Opaque,
            
                
        };
        icalEvent.Alarms.Add(alarm);
        icalEvent.Attendees.Add(new Attendee { CommonName = "Guest", Type = "INDIVIDUAL", ParticipationStatus = EventParticipationStatus.Accepted, Role = ParticipationRole.OptionalParticipant});
        calendar.Events.Add(icalEvent);
        
        var serializer = new CalendarSerializer();
        var icalString = serializer.SerializeToString(calendar);
        
        var path = await SaveIcalToInternalStorageAsync(icalString);
        await UploadOrSendFile(path);
    }
    
    private async Task<string> SaveIcalToInternalStorageAsync(string icalString, string fileName = "events.ics")
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(icalString);

        return filePath;
    }
    
    private async Task UploadOrSendFile(string path)
    {
        var fileUrl = await _fileHostingService.UploadFileAsync(path, "guest@kommunist.dev");
        await Launcher.OpenAsync(fileUrl.Trim());
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

    private string ConvertDateTime(long dt)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(dt).UtcDateTime;
        
        return dateTimeOffset.ToString("yyyyMMdd'T'HHmmss");
    }

    void SetMainCalEventDetail(CalEventDetail eventDetail)
    {
        var mainPart = PageItems.FirstOrDefault(x => x.Type == "Main");
        if (mainPart?.Properties == null) return;
        
        eventDetail.EventId = TappedEventId;
        eventDetail.Title = mainPart.Properties?.Text?.First()?.Text;
        eventDetail.BgImageUrl = mainPart.Properties?.Image.Url;
        eventDetail.PeriodDateTime = GetEventPeriod(mainPart.Properties?.Details.DatesTimestamp.Start, mainPart.Properties?.Details.DatesTimestamp.End);
        eventDetail.Date = mainPart.Properties?.Details.DatesTimestamp.Start.ToLocalDateTime().Date.ToString("d MMM");
        eventDetail.Url = mainPart.Properties?.EventUrl;
        if (mainPart.Properties != null && mainPart.Properties.Languages.Any())
            eventDetail.Language = string.Join(", ", mainPart.Properties.Languages);
        eventDetail.FormatEvent = mainPart.Properties!.Details.ParticipationFormat.Online ? "Online" : "Offline";
        eventDetail.Location = mainPart.Properties.Details.ParticipationFormat.Location ?? "World";

        string text = string.Empty;
        var unlimitedText = PageItems.FirstOrDefault(x => x.Type == "UnlimitedText");
        if (unlimitedText != null)
        {
            text = unlimitedText.Properties?.UnlimitedText;
        }
        
        var iconPointsPart = PageItems.FirstOrDefault(x => x.Type == "IconPoints");
        if (iconPointsPart != null)
        {
            var texts = iconPointsPart.Properties.Text.Select(x => $"<p>{x.Text}</p>");
            var icons = iconPointsPart.Properties.Icons.Select(x => x.Text).ToList();
            if (string.IsNullOrEmpty(text))
            {
                text = string.Join("\n", texts);
            }
            else if (icons.Any())
            {
                text += "<ul>";
                text = icons.Aggregate(text, (current, icon) => current + ("<li>" + icon.Main + "</li>" + "<p>" + icon.Description + "</p>"));
                text += "</ul>";
            }
            else
            {
                text += "\n" + string.Join("\n", texts);
            }
        }

        if (!string.IsNullOrEmpty(text))
        {
            eventDetail.Description = BuildHtmlContent(text);
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
                eventDetail.Description = BuildHtmlContent(agendaItem?.Info?.DescriptionHtml).Trim();
            }
        }
    }
    
    private string BuildHtmlContent(string text)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
            <style>
                body {{
                    margin: 0;
                    padding: 0;
                    overflow: hidden !important;
                    font-family: -apple-system, system-ui;
                }}
            </style>
        </head>
        <body>
            {text}
        </body>
        </html>";
    }
}