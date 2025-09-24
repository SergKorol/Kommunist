using System.Text;
using Kommunist.Core.ApiModels;
using Kommunist.Core.ApiModels.PageProperties.Agenda;
using Kommunist.Core.Converters;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class EventService(HttpClient httpClient, IFilterService filterService) : IEventService
{
    public async Task<IEnumerable<ServiceEvent>?> LoadEvents(DateTime startDate, DateTime endDate)
    {
        var filters = filterService.GetFilters();
        var fromDate = EncodeDateString(startDate.ToString("MM/dd/yyyy"));
        var toDate = EncodeDateString(endDate.ToString("MM/dd/yyyy"));

        var urlParams = new Dictionary<string, string>
        {
            ["start_date"] = fromDate,
            ["end_date"] = toDate
        };

        var url = BuildUrl("/api/v2/calendar", urlParams, filters);

        try
        {
            return await SendRequestAsync<List<ServiceEvent>>(url);
        }
        catch (Exception e)
        {
            LogError(e, "LoadEvents");
            return new List<ServiceEvent>();
        }
    }

    public async Task<IEnumerable<PageItem>?> GetHomePage(int eventId)
    {
        var url = $"/api/v2/events/{eventId}/pages/home";
        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ImageDetailsConverter() }
            };

            return await SendRequestAsync<IEnumerable<PageItem>>(url, settings);
        }
        catch (Exception e)
        {
            LogError(e, "GetHomePage");
            return [];
        }
    }

    public async Task<AgendaPage?> GetAgenda(int eventId)
    {
        var url = $"/api/v2/events/{eventId}/agenda";
        try
        {
            return await SendRequestAsync<AgendaPage>(url);
        }
        catch (Exception e)
        {
            LogError(e, "GetAgenda");
            return null;
        }
    }
    private static string EncodeDateString(string date)
    {
        return Uri.EscapeDataString(date).Replace(".", "%2F");
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

    private async Task<T?> SendRequestAsync<T>(string url, JsonSerializerSettings? settings = null)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return settings != null 
            ? JsonConvert.DeserializeObject<T>(json, settings) 
            : JsonConvert.DeserializeObject<T>(json);
    }

    private static string BuildUrl(string baseUrl, Dictionary<string, string> parameters, FilterOptions? filters = null)
    {
        var url = baseUrl;
        var isFirstParam = !url.Contains('?');

        foreach (var param in parameters)
        {
            url += isFirstParam ? $"?{param.Key}={param.Value}" : $"&{param.Key}={param.Value}";
            isFirstParam = false;
        }

        if (filters != null)
        {
            ApplyFilters(ref url, filters);
        }

        return url;
    }

    private static void ApplyFilters(ref string url, FilterOptions filters)
    {
        SetTagFilters(ref url, filters.TagFilters);
        SetSpeakerFilters(ref url, filters.SpeakerFilters);
        SetCountryFilters(ref url, filters.CountryFilters);
        SetCommunityFilters(ref url, filters.CommunityFilters);
        SetOnlineFilters(ref url, filters.OnlineOnly);
    }

    private static void LogError(Exception exception, string methodName)
    {
        Console.WriteLine($"Error in {methodName}: {exception.Message}");
        #if DEBUG
        Console.WriteLine(exception.StackTrace);
        #endif
    }
}