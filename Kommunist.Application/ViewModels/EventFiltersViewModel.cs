using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.Helpers;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Types;
using Microsoft.Maui.Controls.Internals;

namespace Kommunist.Application.ViewModels;

[Preserve(AllMembers = true)]
public class EventFiltersViewModel : BaseViewModel, IDisposable
{
    #region Services
    private readonly ISearchService _searchService;
    private readonly IFilterService _filterService;
    #endregion
    
    #region Properties
    private string? SelectedTagFilter { get; set; }
    private string? SelectedSpeakerFilter { get; set; }
    private string? SelectedCountryFilter { get; set; }
    private string? SelectedCommunityFilter { get; set; }

    private ObservableCollection<string>? Countries { get; set; }
    #endregion
    
    #region Fields
    private string? _tagFilter;
    private string? _speakerFilter;
    private string? _countryFilter;
    private string? _communityFilter;
    private bool _onlineOnly;
    
    private CancellationTokenSource? _tagDebounceCts;
    private CancellationTokenSource? _speakerDebounceCts;
    private CancellationTokenSource? _countryDebounceCts;
    private CancellationTokenSource? _communityDebounceCts;
    #endregion

    #region Ctor

    public EventFiltersViewModel(ISearchService searchService, IFilterService filterService)
    {
        InitializeCountries();

        _searchService = searchService;
        _filterService = filterService;

        ApplyFiltersCommand = new Command(ExecuteApplyFilters);
        ClearFiltersCommand = new Command(ExecuteClearFilters);
        DeleteFiltersCommand = new Command(ExecuteDeleteFilters);

        // Initialize all tag commands
        SelectTagCommand = new Command<string>(tag =>
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            TagFilter = tag;
            SelectedTagFilter = tag;
            if (!SelectedTags.Contains(tag))
                SelectedTags.Add(tag);
            TagSuggestions.Clear();
        });

        DeselectTagCommand = new Command<string>(tag =>
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            SelectedTags.Remove(tag);
        });

        ClearTagFilterCommand = new Command(() => TagFilter = string.Empty);

        // Initialize all speaker commands
        SelectSpeakerCommand = new Command<string>(speaker =>
        {
            if (string.IsNullOrWhiteSpace(speaker)) return;
            SpeakerFilter = speaker;
            SelectedSpeakerFilter = speaker;
            if (!SelectedSpeakers.Contains(speaker))
                SelectedSpeakers.Add(speaker);
            SpeakerSuggestions.Clear();
        });

        DeselectSpeakerCommand = new Command<string>(speaker =>
        {
            if (string.IsNullOrWhiteSpace(speaker)) return;
            SelectedSpeakers.Remove(speaker);
        });

        ClearSpeakerFilterCommand = new Command(() => SpeakerFilter = string.Empty);

        // Initialize all country commands
        SelectCountryCommand = new Command<string>(country =>
        {
            if (string.IsNullOrWhiteSpace(country)) return;
            CountryFilter = country;
            SelectedCountryFilter = country;
            if (!SelectedCountries.Contains(country))
                SelectedCountries.Add(country);
            CountrySuggestions.Clear();
        });

        DeselectCountryCommand = new Command<string>(country =>
        {
            if (string.IsNullOrWhiteSpace(country)) return;
            SelectedCountries.Remove(country);
        });

        ClearCountryFilterCommand = new Command(() => CountryFilter = string.Empty);

        // Initialize all community commands
        SelectCommunityCommand = new Command<string>(community =>
        {
            if (string.IsNullOrWhiteSpace(community)) return;
            CommunityFilter = community;
            SelectedCommunityFilter = community;
            if (!SelectedCommunities.Contains(community))
                SelectedCommunities.Add(community);
            CommunitySuggestions.Clear();
        });

        DeselectCommunityCommand = new Command<string>(community =>
        {
            if (string.IsNullOrWhiteSpace(community)) return;
            SelectedCommunities.Remove(community);
        });

        ClearCommunityFilterCommand = new Command(() => CommunityFilter = string.Empty);
    }

    #endregion

    #region Commands
    [Preserve]
    public ICommand ApplyFiltersCommand { get; }
    [Preserve]
    public ICommand ClearFiltersCommand { get; }
    [Preserve]
    public ICommand DeleteFiltersCommand { get; }

    [Preserve]
    public Command<string> SelectTagCommand { get; }
    [Preserve]
    public Command<string> DeselectTagCommand { get; }
    [Preserve]
    public Command ClearTagFilterCommand { get; }

    [Preserve]
    public Command<string> SelectSpeakerCommand { get; }
    [Preserve]
    public Command<string> DeselectSpeakerCommand { get; }
    [Preserve]
    public Command ClearSpeakerFilterCommand { get; }

    [Preserve]
    public Command<string> SelectCountryCommand { get; }
    [Preserve]
    public Command<string> DeselectCountryCommand { get; }
    [Preserve]
    public Command ClearCountryFilterCommand { get; }

    [Preserve]
    public Command<string> SelectCommunityCommand { get; }
    [Preserve]
    public Command<string> DeselectCommunityCommand { get; }
    [Preserve]
    public Command ClearCommunityFilterCommand { get; }

    #endregion

    #region Suggestion collections and selected filters

    public ObservableCollection<string> TagSuggestions { get; } = [];
    public ObservableCollection<string> SpeakerSuggestions { get; } = [];
    public ObservableCollection<string> CountrySuggestions { get; } = [];
    public ObservableCollection<string> CommunitySuggestions { get; } = [];

    public ObservableHashSet<string> SelectedTags { get; } = [];
    public ObservableHashSet<string> SelectedSpeakers { get; } = [];
    public ObservableHashSet<string> SelectedCountries { get; } = [];
    public ObservableHashSet<string> SelectedCommunities { get; } = [];

    #endregion

    #region UI helpers

    public bool IsTagFilterNotEmpty => !string.IsNullOrEmpty(TagFilter);
    public bool IsSpeakerFilterNotEmpty => !string.IsNullOrEmpty(SpeakerFilter);
    public bool IsCountryFilterNotEmpty => !string.IsNullOrEmpty(CountryFilter);
    public bool IsCommunityFilterNotEmpty => !string.IsNullOrEmpty(CommunityFilter);

    #endregion

    #region Filter properties

    public string? TagFilter
    {
        get => _tagFilter;
        set
        {
            if (_tagFilter == value) return;

            if (!string.IsNullOrWhiteSpace(SelectedTagFilter) && SelectedTagFilter != value)
                SelectedTagFilter = null;

            _tagFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsTagFilterNotEmpty));

            Debounce(ref _tagDebounceCts, SearchTags);
        }
    }

    public string? SpeakerFilter
    {
        get => _speakerFilter;
        set
        {
            if (_speakerFilter == value) return;

            if (!string.IsNullOrWhiteSpace(SelectedSpeakerFilter) && SelectedSpeakerFilter != value)
                SelectedSpeakerFilter = null;

            _speakerFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSpeakerFilterNotEmpty));

            Debounce(ref _speakerDebounceCts, SearchSpeakers);
        }
    }

    public string? CountryFilter
    {
        get => _countryFilter;
        set
        {
            if (_countryFilter == value) return;

            if (!string.IsNullOrWhiteSpace(SelectedCountryFilter) && SelectedCountryFilter != value)
                SelectedCountryFilter = null;

            _countryFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCountryFilterNotEmpty));

            Debounce(ref _countryDebounceCts, SearchCountries);
        }
    }

    public string? CommunityFilter
    {
        get => _communityFilter;
        set
        {
            if (_communityFilter == value) return;

            if (!string.IsNullOrWhiteSpace(SelectedCommunityFilter) && SelectedCommunityFilter != value)
                SelectedCommunityFilter = null;

            _communityFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCommunityFilterNotEmpty));

            Debounce(ref _communityDebounceCts, SearchCommunities);
        }
    }

    public bool OnlineOnly
    {
        get => _onlineOnly;
        set
        {
            if (_onlineOnly == value) return;
            _onlineOnly = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Initialization

    private void InitializeCountries()
    {
        Countries =
        [
            "🇦🇲 Armenia",
            "🇧🇾 Belarus",
            "🇧🇬 Bulgaria",
            "🇨🇦 Canada",
            "🇨🇱 Chile",
            "🇨🇳 China",
            "🇨🇴 Colombia",
            "🇭🇷 Croatia",
            "🇨🇿 Czechia",
            "🇬🇪 Georgia",
            "🇩🇪 Germany",
            "🇭🇰 Hong Kong",
            "🇭🇺 Hungary",
            "🇮🇳 India",
            "🇯🇵 Japan",
            "🇰🇿 Kazakhstan",
            "🇰🇬 Kyrgyzstan",
            "🇱🇻 Latvia",
            "🇱🇹 Lithuania",
            "🇲🇾 Malaysia",
            "🇲🇽 Mexico",
            "🇲🇪 Montenegro",
            "🇳🇱 Netherlands",
            "🇳🇬 Nigeria",
            "🇵🇱 Poland",
            "🇵🇹 Portugal",
            "🇷🇴 Romania",
            "🇷🇸 Serbia",
            "🇸🇬 Singapore",
            "🇸🇰 Slovakia",
            "🇪🇸 Spain",
            "🇨🇭 Switzerland",
            "🇹🇷 Türkiye",
            "🇺🇦 Ukraine",
            "🇬🇧 United Kingdom",
            "🇺🇸 United States",
            "🇺🇾 Uruguay",
            "🇺🇿 Uzbekistan",
            "🇻🇳 Vietnam"
        ];
    }

    #endregion

    #region Command handlers

    private async void ExecuteApplyFilters()
    {
        try
        {
            var filters = new FilterOptions
            {
                TagFilters = SelectedTags.ToList(),
                SpeakerFilters = SelectedSpeakers.ToList(),
                CountryFilters = SelectedCountries.ToList().WithoutFlags(),
                CommunityFilters = SelectedCommunities.ToList(),
                OnlineOnly = OnlineOnly
            };

            ApplyFilters(filters);

            await Toast.Make("Filters Applied").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't applied: {e.Message}").Show();
        }
    }

    private async void ExecuteClearFilters()
    {
        try
        {
            ResetAllFiltersState();
            await Toast.Make("Filters Cleared").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't cleared: {e.Message}").Show();
        }
    }

    private async void ExecuteDeleteFilters()
    {
        try
        {
            ResetAllFiltersState();
            DeleteFilters();
            await Toast.Make("Filters Deleted").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't deleted: {e.Message}").Show();
        }
    }

    private void ResetAllFiltersState()
    {
        TagFilter = string.Empty;
        SpeakerFilter = string.Empty;
        CountryFilter = string.Empty;
        CommunityFilter = string.Empty;
        OnlineOnly = false;

        SelectedTags.Clear();
        SelectedSpeakers.Clear();
        SelectedCountries.Clear();
        SelectedCommunities.Clear();

        TagSuggestions.Clear();
        SpeakerSuggestions.Clear();
        CountrySuggestions.Clear();
        CommunitySuggestions.Clear();
    }

    private void ApplyFilters(FilterOptions filters)
    {
        _filterService.SetFilters(filters);
    }

    private void DeleteFilters()
    {
        _filterService.ClearFilters();
    }

    #endregion

    #region Debounce and searches

    private static void Debounce(ref CancellationTokenSource? cts, Func<Task> action)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                    await action();
            }
            catch (TaskCanceledException)
            {
            }
        }, token);
    }

    private async Task SearchTags()
    {
        var query = TagFilter?.Trim();
        if (!string.IsNullOrWhiteSpace(SelectedTagFilter) || string.IsNullOrWhiteSpace(query))
        {
            MainThread.BeginInvokeOnMainThread(TagSuggestions.Clear);
            return;
        }

        var tags = await _searchService.GetTags(query);
        var filtered = tags.Where(t => !SelectedTags.Contains(t)).ToList();

        if (!string.Equals(TagFilter?.Trim(), query, StringComparison.Ordinal))
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            TagSuggestions.Clear();
            foreach (var tag in filtered)
                TagSuggestions.Add(tag);
        });
    }

    private async Task SearchSpeakers()
    {
        var query = SpeakerFilter?.Trim();
        if (!string.IsNullOrWhiteSpace(SelectedSpeakerFilter) || string.IsNullOrWhiteSpace(query))
        {
            MainThread.BeginInvokeOnMainThread(SpeakerSuggestions.Clear);
            return;
        }

        var speakers = await _searchService.GetSpeakers(query);
        var filtered = speakers.Where(s => !SelectedSpeakers.Contains(s)).ToList();

        if (!string.Equals(SpeakerFilter?.Trim(), query, StringComparison.Ordinal))
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SpeakerSuggestions.Clear();
            foreach (var speaker in filtered)
                SpeakerSuggestions.Add(speaker);
        });
    }

    private Task SearchCountries()
    {
        var query = CountryFilter?.Trim();
        if (!string.IsNullOrWhiteSpace(SelectedCountryFilter) || string.IsNullOrWhiteSpace(query))
        {
            MainThread.BeginInvokeOnMainThread(CountrySuggestions.Clear);
            return Task.CompletedTask;
        }

        if (Countries == null) return Task.CompletedTask;
        var filteredCountries = Countries
            .Where(c => c.Contains(query, StringComparison.CurrentCultureIgnoreCase) && !SelectedCountries.Contains(c))
            .ToList();

        if (!string.Equals(CountryFilter?.Trim(), query, StringComparison.Ordinal))
            return Task.CompletedTask;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CountrySuggestions.Clear();
            foreach (var country in filteredCountries)
                CountrySuggestions.Add(country);
        });

        return Task.CompletedTask;
    }

    private async Task SearchCommunities()
    {
        var query = CommunityFilter?.Trim();
        if (!string.IsNullOrWhiteSpace(SelectedCommunityFilter) || string.IsNullOrWhiteSpace(query))
        {
            MainThread.BeginInvokeOnMainThread(CommunitySuggestions.Clear);
            return;
        }

        var communities = await _searchService.GetCommunities(query);
        var filtered = communities.Where(c => !SelectedCommunities.Contains(c)).ToList();

        if (!string.Equals(CommunityFilter?.Trim(), query, StringComparison.Ordinal))
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CommunitySuggestions.Clear();
            foreach (var community in filtered)
                CommunitySuggestions.Add(community);
        });
    }

    #endregion

    #region Persistence

    public void LoadFilters()
    {
        var filters = _filterService.GetFilters();
        if (filters == null) return;

        ReplaceItems(SelectedTags, filters.TagFilters);
        ReplaceItems(SelectedSpeakers, filters.SpeakerFilters);
        ReplaceItems(SelectedCountries, MapCountryFiltersToFlags(filters.CountryFilters));
        ReplaceItems(SelectedCommunities, filters.CommunityFilters);
        OnlineOnly = filters.OnlineOnly;
    }

    private IEnumerable<string> MapCountryFiltersToFlags(IEnumerable<string> countries)
    {
        return countries
            .Select(name => Countries?.FindWithFlag(name))
            .OfType<string>();
    }


    private static void ReplaceItems<T>(ObservableHashSet<T> target, IEnumerable<T>? source)
    {
        target.Clear();
        
        foreach (var item in source ?? [])
            target.Add(item);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _tagDebounceCts?.Cancel();
        _tagDebounceCts?.Dispose();
        _speakerDebounceCts?.Cancel();
        _speakerDebounceCts?.Dispose();
        _countryDebounceCts?.Cancel();
        _countryDebounceCts?.Dispose();
        _communityDebounceCts?.Cancel();
        _communityDebounceCts?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}