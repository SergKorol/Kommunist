using System.Text;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Kommunist.Application.Models;

namespace Kommunist.Application.ViewModels;

public class ICalConfigViewModel : BaseViewModel, IQueryAttributable
{
    public List<CalEvent> Events { get; set; } = new();
    public string Invitees { get; set; } = string.Empty;
    public int AlarmMinutes { get; set; } = 10;
    public string Notes { get; set; } = string.Empty;

    public ICommand GenerateIcalCommand { get; }

    public ICalConfigViewModel()
    {
        GenerateIcalCommand = new Command(SaveIcalFile);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("SelectedEvents"))
            Events = query["SelectedEvents"] as List<CalEvent> ?? new List<CalEvent>();
    }
    
    

    private async void SaveIcalFile()
    {
        // Generate the iCal content
        var calendar = new Ical.Net.Calendar();
        foreach (var ev in Events)
        {
            var alarm = new Alarm
            {
                Trigger = new Ical.Net.DataTypes.Trigger(TimeSpan.FromMinutes(-AlarmMinutes).ToString())
            };

            var icalEvent = new CalendarEvent
            {
                Start = new CalDateTime(ev.DateTime),
                Summary = ev.Title,
                Description = $"{ev.Description}\n{Notes}"
            };
            icalEvent.Alarms.Add(alarm);
            calendar.Events.Add(icalEvent);
        }

        var serializer = new CalendarSerializer();
        var icalString = serializer.SerializeToString(calendar);
        
        // Convert the iCal string to a stream
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(icalString));

        // Save the file
        var fileSaverResult = await FileSaver.Default.SaveAsync("events.ics", stream);
        if (fileSaverResult.IsSuccessful)
        {
            await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show();
        }
        else
        {
            await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show();
        }
    }

    
    // private async void GenerateIcal()
    // {
    //     var calendar = new Ical.Net.Calendar();
    //     foreach (var ev in Events)
    //     {
    //         var alarm = new Alarm
    //         {
    //             Trigger = new Ical.Net.DataTypes.Trigger(TimeSpan.FromMinutes(-AlarmMinutes).ToString())
    //         };
    //         
    //         
    //         var icalEvent = new CalendarEvent
    //         {
    //             Start = new CalDateTime(ev.DateTime),
    //             Summary = ev.Title,
    //             Description = $"{ev.Description}\n{Notes}"
    //         };
    //         icalEvent.Alarms.Add(alarm);
    //         calendar.Events.Add(icalEvent);
    //     }
    //
    //     var serializer = new CalendarSerializer();
    //     var icalString = serializer.SerializeToString(calendar);
    //
    //     var eventsFolder = Path.Combine(FileSystem.AppDataDirectory, "Events");
    //
    //     if (!Directory.Exists(eventsFolder))
    //     {
    //         Directory.CreateDirectory(eventsFolder);
    //     }
    //
    //     var filePath = Path.Combine(eventsFolder, "events.ics");
    //     await File.WriteAllTextAsync(filePath, icalString);
    //
    //
    //     await Shell.Current.DisplayAlert("Success", "iCal file generated!", "OK");
    // }
}
