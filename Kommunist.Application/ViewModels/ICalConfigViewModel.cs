using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using JetBrains.Annotations;
using Kommunist.Application.Models;
using Kommunist.Application.Services.Dialog;
using Kommunist.Application.Services.File;
using Kommunist.Application.Services.Launch;
using Kommunist.Application.Services.Toasts;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.ViewModels;

public partial class CalConfigViewModel : ObservableValidator, IQueryAttributable
{
    #region Services
    private readonly IFileHostingService _fileHostingService;
    private readonly IEmailService _emailService;
    private readonly ICoordinatesService _coordinatesService;
    [UsedImplicitly] private readonly IAndroidCalendarService _androidCalendarService;
    private readonly IToastService _toastService;
    private readonly IFileSaverService _fileSaverService;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILauncherService _launcherService;
    [UsedImplicitly] private readonly IPageDialogService _pageDialogService;
    #endregion
    
    #region Properties
    public List<CalEvent> Events { get; set; } = [];
    public int AlarmMinutes { get; set; } = 10;
    public string? Email { get; set; }
    public string? FirstEventDateTime { get; set; }
    public string? LastEventDateTime { get; set; }
    
    public bool SendEmail
    {
        get => _sendEmail;
        set
        {
            if (_sendEmail == value) return;
            _sendEmail = value;
            OnPropertyChanged();
        }
    }
    
    public bool SaveFile
    {
        get => _saveFile;
        set
        {
            if (_saveFile == value) return;
            _saveFile = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Fields
    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    private string? _invitees;
    private bool _sendEmail;
    private bool _saveFile;
    #endregion

    #region Commands
    public ICommand IncrementAlarmCommand => new Command(() => AlarmMinutes = Math.Min(120, AlarmMinutes + 5));
    public ICommand DecrementAlarmCommand => new Command(() => AlarmMinutes = Math.Max(0, AlarmMinutes - 5));
    public ICommand GenerateIcalCommand { get; }
    #endregion
    
    #region Ctor
    public CalConfigViewModel(
        IFileHostingService fileHostingService,
        IEmailService emailService,
        ICoordinatesService coordinatesService,
        IAndroidCalendarService androidCalendarService)
        : this(
            fileHostingService,
            emailService,
            coordinatesService,
            androidCalendarService,
            new ToastService(),
            new FileSaverService(),
            new FileSystemService(),
            new LauncherService(),
            new PageDialogService())
    {
    }

    public CalConfigViewModel(
        IFileHostingService fileHostingService,
        IEmailService emailService,
        ICoordinatesService coordinatesService,
        IAndroidCalendarService androidCalendarService,
        IToastService toastService,
        IFileSaverService fileSaverService,
        IFileSystemService fileSystemService,
        ILauncherService launcherService,
        IPageDialogService pageDialogService
        )
    {
        _fileHostingService = fileHostingService;
        _emailService = emailService;
        _coordinatesService = coordinatesService;
        _androidCalendarService = androidCalendarService;

        _toastService = toastService;
        _fileSaverService = fileSaverService;
        _fileSystemService = fileSystemService;
        _launcherService = launcherService;
        _pageDialogService = pageDialogService;
        
        GenerateIcalCommand = new Command(SaveIcalFile);
    }
    #endregion

    #region Public
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("SelectedEvents", out var events)) return;
        Events = events as List<CalEvent> ?? [];
        var dates = Events.Select(x => x.DateTime).ToList(); 
        FirstEventDateTime = dates.Min().Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture);
        LastEventDateTime = dates.Max().Date.ToString("d MMM yyyy", CultureInfo.CurrentCulture);
    }
    #endregion

    #region Private
    private async void SaveIcalFile()
    {
        try
        {
            await SaveAndValidateIcalFileAsync();
        }
        catch (Exception e)
        {
            await _toastService.ShowAsync($"ICal file wasn't saved: {e.Message}");
        }
    }

    internal async Task SaveAndValidateIcalFileAsync()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            var firstError = GetErrors(nameof(Invitees)).FirstOrDefault()?.ErrorMessage;

            if (!string.IsNullOrEmpty(firstError))
            {
                await _toastService.ShowAsync(firstError);
            }
            else
            {
                await _toastService.ShowAsync("Validation failed.");
            }
            return;
        }

        var calendar = new Ical.Net.Calendar
        {
            Method = "PUBLISH",
            Scale = "GREGORIAN"
        };
        calendar.TimeZones.Add(new VTimeZone { TzId = TimeZoneInfo.Local.Id });

        foreach (var ev in Events)
        {
            var alarm = new Alarm
            {
                Action = AlarmAction.Display,
                Description = "Reminder",
                Trigger = new Ical.Net.DataTypes.Trigger($"-PT{AlarmMinutes}M")
            };

            (double Latitude, double Longitude) coordinates = (0, 0);
            if (ev.Location != null)
            {
                coordinates = await _coordinatesService.GetCoordinatesAsync(ev.Location);
            }

            var icalEvent = new CalendarEvent
            {
                Start = new CalDateTime(ConvertDateTime(ev.Start), TimeZoneInfo.Local.Id),
                End = new CalDateTime(ConvertDateTime(ev.End), TimeZoneInfo.Local.Id),
                Summary = ev.Title,
                Location = NormalizeCalendarLocation(ev.Location),
                Description = $"{ev.Description}\n{ev.Url}",
                Transparency = TransparencyType.Opaque
            };

            if (coordinates.Latitude != 0 && coordinates.Longitude != 0)
            {
                icalEvent.GeographicLocation = new GeographicLocation(coordinates.Latitude, coordinates.Longitude);
            }

            icalEvent.Alarms.Add(alarm);

            if (!string.IsNullOrEmpty(Invitees))
            {
                icalEvent.Attendees.Add(new Attendee
                {
                    CommonName = Invitees,
                    Type = "INDIVIDUAL",
                    ParticipationStatus = EventParticipationStatus.Accepted,
                    Role = ParticipationRole.OptionalParticipant,
                    Value = new Uri($"mailto:{Invitees}")
                });
            }

            calendar.Events.Add(icalEvent);
        }

        var serializer = new CalendarSerializer();
        var icalString = serializer.SerializeToString(calendar);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(icalString));

        string? path = null;
        if (SaveFile)
        {
            var fileSaverResult = await _fileSaverService.SaveAsync("events.ics", stream);
            if (fileSaverResult.IsSuccessful)
            {
                #if ANDROID
                path = await SaveIcalToInternalStorageAsync(icalString);
                await UploadOrSendFile(path);    
                #else
                await UploadOrSendFile(fileSaverResult.FilePath);
                #endif
            }
            else
            {
                await _toastService.ShowAsync($"The file was not saved successfully with error: {fileSaverResult.Exception?.Message}");
            }
        }
        else
        {
            path ??= await SaveIcalToInternalStorageAsync(icalString);
            await UploadOrSendFile(path);
        }
    }

    private async Task UploadOrSendFile(string? path)
    {
        if (SendEmail)
        {
            await _emailService.SendEmailAsync(Invitees, "Your iCal Event", GetEmailBody(), path, Email);
            await _toastService.ShowAsync("The file was sent successfully");
        }
        else
        {
            #if ANDROID
            try
            {
                var calendarNames = await _androidCalendarService.GetCalendarNames();
                var chosenName = await _pageDialogService.DisplayActionSheet(
                    "Choose calendar", "Cancel", null, calendarNames);

                if (string.IsNullOrEmpty(chosenName) || chosenName == "Cancel")
                    return;

                await _androidCalendarService.AddEvents(path, chosenName);
            }
            catch (Exception e)
            {
                await _toastService.ShowAsync($"Failed to add event: {e.Message}");
            }
            #else
            var fileUrl = await _fileHostingService.UploadFileAsync(path, Email);
            await _toastService.ShowAsync("The file was uploaded successfully");
            await _launcherService.OpenAsync(fileUrl.Trim());
            #endif
        }
    }

    private async Task<string> SaveIcalToInternalStorageAsync(string icalString, string fileName = "events.ics")
    {
        return await _fileSystemService.SaveTextAsync(fileName, icalString);
    }

    private static DateTime ConvertDateTime(long dt)
    {
        var local = DateTimeOffset.FromUnixTimeSeconds(dt).ToLocalTime().DateTime;
        return DateTime.SpecifyKind(local, DateTimeKind.Local);
    }

    private static string NormalizeCalendarLocation(string? location)
    {
        if (string.IsNullOrEmpty(location)) return string.Empty;
        var locationParts = location.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
        locationParts = locationParts.Reverse().ToArray();
        var normalizedLocation = string.Join(", ", locationParts.Select(x => x.Trim()));
        return normalizedLocation;
    }

    private static string GetEmailBody()
    {
        return """

                               <html>
                                 <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
                                   <table align='center' width='600' cellpadding='0' cellspacing='0' style='background: #ffffff; padding: 20px; border-radius: 8px;'>
                                     <tr>
                                       <td style='text-align: center;'>
                                         <h2 style='color: #333;'>Your iCal Event</h2>
                                         <p style="color:#555; font-size:14px;">
                                          📅 Your calendar invite is attached as <b>events.ics</b>.<br>
                                          Open the attachment to add it to your calendar.
                                           </p>
                                         <p style='margin-top:20px; color: #777; font-size: 12px;'>
                                           If you have any issues opening the event, please reply to this email.
                                         </p>
                                       </td>
                                     </tr>
                                   </table>
                                 </body>
                               </html>
               """;
    }
    #endregion
}