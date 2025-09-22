using System.Text;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using JetBrains.Annotations;
using Kommunist.Application.Helpers;
using Kommunist.Application.Models;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Helpers;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Attendee = Ical.Net.DataTypes.Attendee;
using Trigger = Ical.Net.DataTypes.Trigger;

namespace Kommunist.Application.ViewModels;

public class EventCalendarDetailViewModel : BaseViewModel
{
    private readonly IFileHostingService _fileHostingService;
    [UsedImplicitly] private readonly IAndroidCalendarService _androidCalendarService;
    
    private CalEventDetail? _selectedEventDetail;
    public CalEventDetail? SelectedEventDetail
    {
        get => _selectedEventDetail;
        private set
        {
            if (_selectedEventDetail == value) return;
            _selectedEventDetail = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasParticipants));
        }
    }

    private IEnumerable<PageItem>? PageItems { get; set; }
    private AgendaPage? AgendaPage { get; set; }


    private bool _isWebViewLoading;
    public bool IsWebViewLoading
    {
        get => _isWebViewLoading;
        set
        {
            if (_isWebViewLoading == value) return;
            _isWebViewLoading = value;
            OnPropertyChanged();
        }
    }


    private int TappedEventId { get; }

    private readonly IEventService _eventService;

    public ICommand AddToCalendar { get; }
    public ICommand JoinToEvent { get; }

    public EventCalendarDetailViewModel(IEventService eventService, int tappedEventId, IFileHostingService fileHostingService, IAndroidCalendarService androidCalendarService)
    {
        _eventService = eventService;
        _fileHostingService = fileHostingService;
        _androidCalendarService = androidCalendarService;
        TappedEventId = tappedEventId;

        _ = CreateEventCalendarDetailPage();
        AddToCalendar = new Command(async void () =>
        {
            try
            {
                await GenerateEventAndUpload();
            }
            catch (Exception e)
            {
                await Toast.Make($"Event wasn't added: {e.Message}").Show();
            }
        });
        JoinToEvent = new Command(async void () =>
        {
            try
            {
                await OpenEventPage();
            }
            catch (Exception e)
            {
                await Toast.Make($"Event wasn't opened: {e.Message}").Show();
            }
        });
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
        (SelectedEventDetail.Speakers is { Count: > 0 } ||
         SelectedEventDetail.Moderators is { Count: > 0 });

    private async Task GetAgenda(int eventId)
    {
        if (eventId == 0) return;

        try
        {
            AgendaPage = await _eventService.GetAgenda(eventId);
        }
        catch (Exception e)
        {
            await Toast.Make($"Failed to load agenda: {e.Message}").Show();
        }
    }

    private async Task OpenEventPage()
    {
        try
        {
            var url = SelectedEventDetail?.Url?.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                await Toast.Make("Event page URL is missing").Show();
                return;
            }

            await Launcher.OpenAsync(url);
        }
        catch (Exception e)
        {
            await Toast.Make($"Event page wasn't opened: {e.Message}").Show();
        }
    }

    private async Task GenerateEventAndUpload()
    {
        try
        {
            if (PageItems == null)
            {
                await Toast.Make("Event details are not loaded yet").Show();
                return;
            }

            var properties = PageItems.FirstOrDefault(x => x.Type == "Main")?.Properties;
            if (properties?.Details?.DatesTimestamp == null)
            {
                await Toast.Make("Event date/time is missing").Show();
                return;
            }

            var startUnix = properties.Details.DatesTimestamp.Start;
            var endUnix = properties.Details.DatesTimestamp.End;
            if (startUnix == 0 || endUnix == 0)
            {
                await Toast.Make("Event date/time is invalid").Show();
                return;
            }

            var start = ConvertDateTime(startUnix);
            var end = ConvertDateTime(endUnix);

            // ReSharper disable once RedundantNameQualifier
            var calendar = new Ical.Net.Calendar
            {
                Method = "PUBLISH",
                Scale = "GREGORIAN"
            };
            calendar.TimeZones.Add(new VTimeZone { TzId = TimeZoneInfo.Local.Id });

            var alarm = new Alarm
            {
                Action = AlarmAction.Display,
                Description = "Reminder",
                Trigger = new Trigger("-PT5M")
            };

            var icalEvent = new CalendarEvent
            {
                Uid = Guid.NewGuid().ToString("N"),
                Summary = string.IsNullOrWhiteSpace(SelectedEventDetail?.Title) ? "Event" : SelectedEventDetail.Title,
                Description = BuildIcsDescription(SelectedEventDetail?.Description, SelectedEventDetail?.Url),
                DtStart = new CalDateTime(start, TimeZoneInfo.Local.Id),
                DtEnd = new CalDateTime(end, TimeZoneInfo.Local.Id),
                Transparency = TransparencyType.Opaque
            };

            var location = properties.Details.ParticipationFormat?.Location;
            if (!string.IsNullOrWhiteSpace(location))
            {
                icalEvent.Location = location.Trim();
            }

            var urlText = SelectedEventDetail?.Url?.Trim();
            if (!string.IsNullOrWhiteSpace(urlText) && Uri.TryCreate(urlText, UriKind.Absolute, out var uri))
            {
                icalEvent.Url = uri;
            }

            icalEvent.Alarms.Add(alarm);
            icalEvent.Attendees.Add(new Attendee
            {
                CommonName = "Guest",
                Type = "INDIVIDUAL",
                ParticipationStatus = EventParticipationStatus.Accepted,
                Role = ParticipationRole.OptionalParticipant
            });

            calendar.Events.Add(icalEvent);

            var serializer = new CalendarSerializer();
            var icalString = serializer.SerializeToString(calendar);

            var fileSafeTitle = MakeSafeFileName(SelectedEventDetail?.Title ?? "event");
            var fileName = $"{fileSafeTitle}-{startUnix}.ics";
            var path = await SaveIcalToInternalStorageAsync(icalString, fileName);

            await UploadOrSendFile(path);
        }
        catch (Exception e)
        {
            await Toast.Make($"Event wasn't added: {e.Message}").Show();
        }
    }

    private static async Task<string> SaveIcalToInternalStorageAsync(string icalString, string fileName = "events.ics")
    {
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(icalString);

        return filePath;
    }

    private async Task UploadOrSendFile(string path)
    {
        try
        {
            #if ANDROID
            try
            {
                var calendarNames = await _androidCalendarService.GetCalendarNames();
                var chosenName = await App.Current.MainPage.DisplayActionSheet(
                    "Choose calendar", "Cancel", null, calendarNames);
        
                if (string.IsNullOrEmpty(chosenName) || chosenName == "Cancel")
                    return;
                
                await _androidCalendarService.AddEvents(path, chosenName);
            }
            catch (Exception e)
            {
                await Toast.Make($"Failed to add event: {e.Message}").Show();
            }
            #else
            var fileUrl = await _fileHostingService.UploadFileAsync(path, "guest@kommunist.dev");
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                await Toast.Make("Upload failed: empty URL returned").Show();
                return;
            }

            await Launcher.OpenAsync(fileUrl.Trim());
            #endif
        }
        catch (Exception e)
        {
            await Toast.Make($"File upload/open failed: {e.Message}").Show();
        }
    }

    private async Task GetHomePage(int eventId)
    {
        if (eventId == 0) return;

        try
        {
            PageItems = await _eventService.GetHomePage(eventId);
        }
        catch (Exception e)
        {
            await Toast.Make($"Failed to load event: {e.Message}").Show();
        }
    }

    private static string GetEventPeriod(long? start, long? end)
    {
        if (start == null || end == null) return string.Empty;
        var startDate = start.Value.ToLocalDateTime();
        var endDate = end.Value.ToLocalDateTime();
        if (startDate.Day == endDate.Day)
        {
            return startDate.ToString("d MMM yyyy, HH:mm") + "-" + endDate.ToString("HH:mm");
        }

        return startDate.ToString("d MMM yyyy, HH:mm") + " - " + endDate.ToString("d MMM yyyy, HH:mm");
    }

    private static DateTime ConvertDateTime(long dt)
    {
        // Convert Unix seconds to local DateTime (Ical.Net will apply provided timezone)
        return DateTimeOffset.FromUnixTimeSeconds(dt).LocalDateTime;
    }

    private void SetMainCalEventDetail(CalEventDetail eventDetail)
    {
        var mainPart = PageItems?.FirstOrDefault(x => x.Type == "Main");
        if (mainPart?.Properties == null) return;

        eventDetail.Title = mainPart.Properties.Text?.FirstOrDefault()?.Text;
        eventDetail.BgImageUrl = mainPart.Properties.Image?.Url;
        eventDetail.PeriodDateTime = GetEventPeriod(mainPart.Properties.Details?.DatesTimestamp?.Start, mainPart.Properties.Details?.DatesTimestamp?.End);
        eventDetail.Date = mainPart.Properties.Details?.DatesTimestamp?.Start.ToLocalDateTime().Date.ToString("d MMM");
        eventDetail.Url = mainPart.Properties.EventUrl;

        var languages = mainPart.Properties.Languages;
        if (languages is { Count: > 0 })
            eventDetail.Language = string.Join(", ", languages).ToUpperInvariant();

        eventDetail.FormatEvent = mainPart.Properties.Details?.ParticipationFormat?.Online == true ? "Online" : "Offline";

        eventDetail.Location = mainPart.Properties.Details?.ParticipationFormat?.Location ?? "World";

        var text = string.Empty;
        var unlimitedText = PageItems?.FirstOrDefault(x => x.Type == "UnlimitedText");
        if (unlimitedText != null)
        {
            text = unlimitedText.Properties?.UnlimitedText ?? string.Empty;
        }

        var iconPointsPart = PageItems?.FirstOrDefault(x => x.Type == "IconPoints");
        if (iconPointsPart?.Properties != null)
        {
            var texts = iconPointsPart.Properties.Text?.Select(x => $"<p>{x.Text}</p>") ?? [];
            var icons = iconPointsPart.Properties.Icons?.Select(x => x.Text).ToList() ?? [];

            if (string.IsNullOrEmpty(text))
            {
                text = string.Join("\n", texts);
            }
            else if (icons.Count != 0)
            {
                text += "<ul>";
                text = icons.Aggregate(text, (current, icon) => current + "<li>" + icon.Main + "</li>" + "<p>" + icon.Description + "</p>");
                text += "</ul>";
            }
            else
            {
                text += "\n" + string.Join("\n", texts);
            }
        }

        if (string.IsNullOrEmpty(text)) return;
        var isDark = IsDarkMode();
        eventDetail.Description = isDark ? BuildDarkHtmlContent(text) : BuildLightHtmlContent(text);
    }

    private async Task SetAgendaPage(CalEventDetail eventDetail)
    {
        var agendaPart = PageItems?.FirstOrDefault(x => x.Type == "Agenda");
        if (agendaPart?.Properties == null) return;

        await GetAgenda(TappedEventId);

        if (AgendaPage?.Agenda?.Items?.Any() == true)
        {
            var agendaItem = AgendaPage.Agenda.Items.First();

            if (agendaItem?.Speakers?.Any() == true)
            {
                foreach (var speaker in agendaItem.Speakers)
                {
                    var speakerCard = new PersonCard
                    {
                        Name = speaker.Name,
                        Company = speaker.Company,
                        Position = speaker.JobPosition,
                        Avatar = !string.IsNullOrWhiteSpace(speaker.AvatarSmall) && speaker.AvatarSmall.StartsWith("https") ? speaker.AvatarSmall : "no_photo.png"
                    };
                    eventDetail.Speakers.Add(speakerCard);
                }
            }

            if (agendaItem?.Moderators?.Any() == true)
            {
                foreach (var moderator in agendaItem.Moderators)
                {
                    var moderatorCard = new PersonCard
                    {
                        Name = moderator.Name,
                        Company = moderator.Company,
                        Position = moderator.JobPosition,
                        Avatar = !string.IsNullOrWhiteSpace(moderator.AvatarSmall) && moderator.AvatarSmall.StartsWith("https") ? moderator.AvatarSmall : "no_photo.png"
                    };
                    eventDetail.Moderators.Add(moderatorCard);
                }
            }

            var html = agendaItem?.Info?.DescriptionHtml;
            if (!string.IsNullOrWhiteSpace(html))
            {
                var isDark = IsDarkMode();
                eventDetail.Description = isDark ? BuildDarkHtmlContent(html) : BuildLightHtmlContent(html);
            }
        }
    }

    private static bool IsDarkMode()
    {
        var theme = Microsoft.Maui.Controls.Application.Current?.UserAppTheme;
        if (theme == AppTheme.Unspecified)
            theme = Microsoft.Maui.Controls.Application.Current?.RequestedTheme;
        return theme == AppTheme.Dark;
    }

    private static string BuildLightHtmlContent(string? text)
    {
        return $$"""

                         <!DOCTYPE html>
                         <html>
                         <head>
                             <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
                             <style>
                                 body {
                                     margin: 0;
                                     padding: 0;
                                     overflow: hidden !important;
                                     font-family: -apple-system, system-ui;
                                     text-align: center;
                                 }
                             </style>
                         </head>
                         <body>
                             {{text}}
                         </body>
                         </html>
                 """;
    }

    private static string BuildDarkHtmlContent(string? text)
    {
        return $$"""

                     <!DOCTYPE html>
                     <html>
                     <head>
                         <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
                         <style>
                             body {
                                 margin: 0;
                                 padding: 0;
                                 overflow: hidden !important;
                                 font-family: -apple-system, system-ui;
                                 background-color: #121212;
                                 color: #e0e0e0;
                                 text-align: center;
                             }
                             a {
                                 color: #bb86fc;
                             }
                             img {
                                 filter: brightness(0.8) contrast(1.2);
                             }
                         </style>
                     </head>
                     <body>
                         {{text}}
                     </body>
                     </html>
                 """;
    }

    private static string BuildIcsDescription(string? htmlDescription, string? url)
    {
        var sb = new StringBuilder();
        var plain = HtmlConverter.HtmlToPlainText(htmlDescription ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(plain))
            sb.AppendLine(plain);
        if (string.IsNullOrWhiteSpace(url)) return sb.ToString();
        if (sb.Length > 0) sb.AppendLine();
        sb.Append(url.Trim());
        return sb.ToString();
    }

    private static string MakeSafeFileName(string name)
    {
        var invalids = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            sb.Append(invalids.Contains(ch) ? '_' : ch);
        }
        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? "event" : result;
    }
}
