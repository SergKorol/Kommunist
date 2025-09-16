using System.Text.Json;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Services;

public class FilterService : IFilterService
{
    private const string StorageKey = "AppFilters";

    private readonly IAppPreferences _preferences;
    private FilterOptions _filters;

    public FilterService() : this(new MauiPreferences())
    {
    }

    public FilterService(IAppPreferences preferences)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
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
        _preferences.Remove(StorageKey);
    }

    private void SaveFilters()
    {
        var json = JsonSerializer.Serialize(_filters);
        _preferences.Set(StorageKey, json);
    }

    private void LoadFilters()
    {
        var json = _preferences.Get(StorageKey, null);
        _filters = json is null or ""
            ? new FilterOptions()
            : JsonSerializer.Deserialize<FilterOptions>(json) ?? new FilterOptions();
    }
}
