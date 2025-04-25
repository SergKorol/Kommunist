using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Kommunist.Application.Models;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.ViewModels;

public partial class ICalConfigViewModel : ObservableValidator, IQueryAttributable
{
    private readonly IFileHostingService _fileHostingService;
    private readonly IEmailService _emailService;
    
    public List<CalEvent> Events { get; set; } = new();
    
    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    private string _invitees;
    public int AlarmMinutes { get; set; } = 10;
    
    public string Email { get; set; }
    
    private bool _sendEmail;
    public bool SendEmail
    {
        get => _sendEmail;
        set
        {
            if (_sendEmail != value)
            {
                _sendEmail = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool _saveFile;
    public bool SaveFile
    {
        get => _saveFile;
        set
        {
            if (_saveFile != value)
            {
                _saveFile = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand GenerateIcalCommand { get; }

    public ICalConfigViewModel(IFileHostingService fileHostingService, IEmailService emailService)
    {
        _fileHostingService = fileHostingService;
        _emailService = emailService;
        GenerateIcalCommand = new Command(SaveIcalFile);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("SelectedEvents"))
            Events = query["SelectedEvents"] as List<CalEvent> ?? new List<CalEvent>();
    }
    
    

    private async void SaveIcalFile()
    {
        ValidateAllProperties();
        if (HasErrors)
        {
            var firstError = GetErrors(nameof(Invitees)).FirstOrDefault()?.ErrorMessage;

            if (!string.IsNullOrEmpty(firstError))
            {
                await Toast.Make(firstError).Show();
            }
            else
            {
                await Toast.Make("Validation failed.").Show();
            }
            return;
        }
        
        var calendar = new Ical.Net.Calendar
        {
            Method = "PUBLISH",
            Scale = "GREGORIAN"
        };
        calendar.TimeZones.Add(new VTimeZone { TzId = TimeZoneInfo.Local.Id});
        
        
        foreach (var ev in Events)
        {
            var alarm = new Alarm
            {
                Trigger = new Ical.Net.DataTypes.Trigger($"-PT{AlarmMinutes}M"),
                Description = "Reminder",
                Action = AlarmAction.Display,
            };

            
            var icalEvent = new CalendarEvent
            {
                Start = new CalDateTime(ev.DateTime, TimeZoneInfo.Local.Id),
                Summary = ev.Title,
                Description = $"{ev.Description}\n{ev.Url}",
                DtStart = new CalDateTime(ConvertDateTime(ev.Start), TimeZoneInfo.Local.Id),
                DtEnd = new CalDateTime(ConvertDateTime(ev.End), TimeZoneInfo.Local.Id),
                Transparency = TransparencyType.Opaque,
                
            };
            icalEvent.Alarms.Add(alarm);
            if (!string.IsNullOrEmpty(Invitees))
            {
                icalEvent.Attendees.Add(new Attendee { CommonName = Invitees, Type = "INDIVIDUAL", ParticipationStatus = EventParticipationStatus.Accepted, Role = ParticipationRole.OptionalParticipant});
            }
            calendar.Events.Add(icalEvent);
        }

        var serializer = new CalendarSerializer();
        var icalString = serializer.SerializeToString(calendar);
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(icalString));
        
        
        if (SaveFile)
        {
            var fileSaverResult = await FileSaver.Default.SaveAsync("events.ics", stream);
            if (fileSaverResult.IsSuccessful)
            {
                await UploadOrSendFile(fileSaverResult.FilePath);
            }
            else
            {
                await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show();
            
            }
        }
        else
        {
            var path = await SaveIcalToInternalStorageAsync(icalString);
            await UploadOrSendFile(path);
        }
    }

    private async Task UploadOrSendFile(string path)
    {
        if (SendEmail)
        {
            await _emailService.SendEmailAsync(Invitees, "Your iCal Event", "<p>Find attached your iCal event</p><br>.", path, Email);
            await Toast.Make($"The file was sent successfully").Show();
        }
        else
        {
            var fileUrl = await _fileHostingService.UploadFileAsync(path, Email);
            await Toast.Make($"The file was uploaded successfully").Show();
            await Launcher.OpenAsync(fileUrl.Trim());
        }
    }

    public async Task<string> SaveIcalToInternalStorageAsync(string icalString, string fileName = "events.ics")
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(icalString);

        return filePath;
    }

    private string ConvertDateTime(long dt)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(dt).UtcDateTime;
        
        return dateTimeOffset.ToString("yyyyMMdd'T'HHmmss");
    }
}
