using Kommunist.Core.Entities;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class EventService : IEventService
{
    private readonly HttpClient _httpClient;
    
    public EventService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate)
    {
        string fromDate = EncodeDateString(startDate.ToString("MM/dd/yyyy"));
        string toDate = EncodeDateString(endDate.ToString("MM/dd/yyyy"));
        var url = $"/api/v2/calendar?start_date={fromDate}&end_date={toDate}";
        
        try
        {
            HttpResponseMessage response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
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

    private string EncodeDateString(string date)
    {
        return Uri.EscapeDataString(date).Replace(".", "%2F");
    }
}