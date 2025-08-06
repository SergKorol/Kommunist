using System.Text;
using Kommunist.Core.Converters;
using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class EventService(HttpClient httpClient, IFilterService filterService) : IEventService
{
    public async Task<IEnumerable<ServiceEvent>> LoadEvents(DateTime startDate, DateTime endDate)
    {
        var filters = filterService.GetFilters();
        var fromDate = EncodeDateString(startDate.ToString("MM/dd/yyyy"));
        var toDate = EncodeDateString(endDate.ToString("MM/dd/yyyy"));
        var url = $"/api/v2/calendar?start_date={fromDate}&end_date={toDate}";
        SetFilters(filters, ref url);
        
        try
        {
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonConvert.DeserializeObject<List<ServiceEvent>>(json);
            return events;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return new List<ServiceEvent>();
        }
    }

    public async Task<IEnumerable<PageItem>> GetHomePage(int eventId)
    {
        var url = $"/api/v2/events/{eventId}/pages/home";
        try
        {
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ImageDetailsConverter() }
            };
            var eventPages = JsonConvert.DeserializeObject<IEnumerable<PageItem>>(json, settings);
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
            var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var agenda = JsonConvert.DeserializeObject<AgendaPage>(json);
            return agenda;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return null;
        }
    }
    private static string EncodeDateString(string date)
    {
        return Uri.EscapeDataString(date).Replace(".", "%2F");
    }
    
    private static void SetFilters(FilterOptions filters, ref string url)
    {
        SetTagFilters(ref url, filters.TagFilters);
        SetSpeakerFilters(ref url, filters.SpeakerFilters);
        SetCountryFilters(ref url, filters.CountryFilters);
        SetCommunityFilters(ref url, filters.CommunityFilters);
        SetOnlineFilters(ref url, filters.OnlineOnly);
        
    }
    
    private static void SetTagFilters(ref string url, List<string> filters)
    {
        if (filters.Count == 0) return;
        var sb = new StringBuilder();
        foreach (var filter in filters)
        {
            sb.Append($"&tag[]={filter}");
        }
        
        url += sb;

    }
    
    private static void SetSpeakerFilters(ref string url, List<string> filters)
    {
        if (filters.Count == 0) return;
        var sb = new StringBuilder();
        foreach (var filter in filters)
        {
            sb.Append($"&speaker[]={filter}");
        }
        
        url += sb;
    }
    
    private static void SetCountryFilters(ref string url, List<string> filters)
    {
        if (filters.Count == 0) return;
        var sb = new StringBuilder();
        foreach (var filter in filters)
        {
            sb.Append($"&location[]={filter}");
        }
        
        url += sb;
    }
    
    private static void SetCommunityFilters(ref string url, List<string> filters)
    {
        if (filters.Count == 0) return;
        var sb = new StringBuilder();
        foreach (var filter in filters)
        {
            sb.Append($"&community[]={filter}");
        }
        
        url += sb;
    }
    
    private static void SetOnlineFilters(ref string url, bool isOnline)
    {
        if (isOnline) url += "&online=Online";
    }
}