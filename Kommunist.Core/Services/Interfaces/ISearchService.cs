namespace Kommunist.Core.Services.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<string>> GetTags(string query);
    Task<IEnumerable<string>> GetSpeakers(string query);
}