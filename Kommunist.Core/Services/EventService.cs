using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;
using Event = Kommunist.Core.Entities.Event;

namespace Kommunist.Core.Services;

public class EventService(HttpClient httpClient) : IEventService
{
    public async Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate)
    {
        string fromDate = EncodeDateString(startDate.ToString("MM/dd/yyyy"));
        string toDate = EncodeDateString(endDate.ToString("MM/dd/yyyy"));
        var url = $"/api/v2/calendar?start_date={fromDate}&end_date={toDate}";
        
        try
        {
            HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonConvert.DeserializeObject<List<Event>>(json);
            return events;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return new List<Event>();
        }
    }

    public async Task<IEnumerable<PageItem>> GetHomePage(int eventId)
    {
        var url = $"/api/v2/events/{eventId}/pages/home";
        try
        {
            HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var eventPages = JsonConvert.DeserializeObject<IEnumerable<PageItem>>(json);
            return eventPages;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return [];
        }
    }

    public async Task<AgendaPage> GetAgenda(int eventId)
    {
        var url = $"/api/v2/events/{eventId}/agenda";
        try
        {
            HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var agenda = JsonConvert.DeserializeObject<AgendaPage>(json);
            return agenda;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return default;
        }
    }
    private string EncodeDateString(string date)
    {
        return Uri.EscapeDataString(date).Replace(".", "%2F");
    }
}