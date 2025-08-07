using System.Text.Json;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class FilterService : IFilterService
{
    private const string StorageKey = "AppFilters";

    private FilterOptions Filters { get; set; }

    public FilterService()
    {
        LoadFilters();
    }

    public void SetFilters(FilterOptions filters)
    {
        Filters = filters;
        SaveFilters();
    }

    public FilterOptions GetFilters()
    {
        LoadFilters();
        return Filters;
    }

    public void ClearFilters()
    {
        Filters = new FilterOptions();
        Preferences.Remove(StorageKey);
    }

    private void SaveFilters()
    {
        var json = JsonSerializer.Serialize(Filters);
        Preferences.Set(StorageKey, json);
    }

    private void LoadFilters()
    {
        var json = Preferences.Get(StorageKey, null);
        Filters = string.IsNullOrEmpty(json)
            ? new FilterOptions()
            : JsonSerializer.Deserialize<FilterOptions>(json);
    }
}
