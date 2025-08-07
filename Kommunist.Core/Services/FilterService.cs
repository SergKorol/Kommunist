using System.Text.Json;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class FilterService : IFilterService
{
    private const string StorageKey = "AppFilters";

    private FilterOptions _filters;

    public FilterService()
    {
        LoadFilters();
    }

    public void SetFilters(FilterOptions filters)
    {
        _filters = filters;
        SaveFilters();
    }

    public FilterOptions GetFilters()
    {
        LoadFilters();
        return _filters;
    }

    public void ClearFilters()
    {
        _filters = new FilterOptions();
        Preferences.Remove(StorageKey);
    }

    private void SaveFilters()
    {
        var json = JsonSerializer.Serialize(_filters);
        Preferences.Set(StorageKey, json);
    }

    private void LoadFilters()
    {
        var json = Preferences.Get(StorageKey, null);
        _filters = json is null or ""
            ? new FilterOptions()
            : JsonSerializer.Deserialize<FilterOptions>(json) ?? new FilterOptions();
    }
}
