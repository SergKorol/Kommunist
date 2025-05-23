using Kommunist.Core.Entities;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class SearchService(HttpClient httpClient) : ISearchService
{
    public async Task<IEnumerable<string>> GetTags(string query)
    {
        var url = $"/api/v2/dictionaries/skills/search?search_query={query}";

        try
        {
            HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tags = JsonConvert.DeserializeObject<List<string>>(json);
            return tags;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An HTTP request error occurred: {e.Message}");
            return new List<string>();
        }
    }
}