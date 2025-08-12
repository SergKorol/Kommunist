using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class SearchService(HttpClient httpClient) : ISearchService
{
    public Task<IEnumerable<string>> GetTags(string query) => 
        FetchSearchResults("/api/v2/dictionaries/skills/search", query);

    public Task<IEnumerable<string>> GetSpeakers(string query) => 
        FetchSearchResults("/api/v2/speakers/search", query);

    public Task<IEnumerable<string>> GetCommunities(string query) => 
        FetchSearchResults("/api/v2/communities/search", query);

    private async Task<IEnumerable<string>> FetchSearchResults(string endpoint, string query)
    {
        var url = $"{endpoint}?search_query={query}";

        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<string>>(json);
            return results ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return new List<string>();
        }
    }
}