using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Helpers;
using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Types;

namespace Kommunist.Application.ViewModels;

public class EventFiltersViewModel : BaseViewModel
{

    #region Properties

    private readonly ISearchService _searchService;
    private readonly IFilterService _filterService;
    private string _tagFilter;
    private string _speakerFilter;
    private string _countryFilter;
    private string _communityFilter;
    private bool _onlineOnly;
    
    public ObservableCollection<string> TagSuggestions { get; } = [];
    public ObservableCollection<string> SpeakerSuggestions { get; } = [];
    public ObservableCollection<string> CountrySuggestions { get; } = [];
    public ObservableCollection<string> CommunitySuggestions { get; } = [];

    public ObservableHashSet<string> SelectedTags { get; set; } = [];
    
    public ObservableHashSet<string> SelectedSpeakers { get; set; } = [];
    public ObservableHashSet<string> SelectedCountries { get; set; } = [];
    public ObservableHashSet<string> SelectedCommunities { get; set; } = [];
    
    private ObservableCollection<string> Countries { get; set; }

    private CancellationTokenSource _debounceCts;

    #endregion
    

    public ICommand ApplyFiltersCommand { get; private set; }
    public ICommand ClearFiltersCommand { get; private set; }
    public ICommand DeleteFiltersCommand { get; private set; }
    
    public Command<string> SelectTagCommand => new(tag =>
    {
        TagFilter = tag;
        SelectedTagFilter = tag;
        SelectedTags.Add(tag);
        TagSuggestions.Clear();
    });
    
    public Command<string> DeselectTagCommand => new(tag =>
    {
        SelectedTags.Remove(tag);
    });
    
    public Command ClearTagFilterCommand => new(_ => TagFilter = string.Empty);

    public Command<string> SelectSpeakerCommand => new(speaker =>
    {
        SpeakerFilter = speaker;
        SelectedSpeakerFilter = speaker;
        SelectedSpeakers.Add(speaker);
        SpeakerSuggestions.Clear();
    });
    
    public Command<string> DeselectSpeakerCommand => new(speaker =>
    {
        SelectedSpeakers.Remove(speaker);
    });
    
    public Command ClearSpeakerFilterCommand => new(_ => SpeakerFilter = string.Empty);

    public Command<string> SelectCountryCommand => new(country =>
    {
        CountryFilter = country;
        SelectedCountryFilter = country;
        SelectedCountries.Add(country);
        CountrySuggestions.Clear();
    });
    
    public Command<string> DeselectCountryCommand => new(country =>
    {
        SelectedCountries.Remove(country);
    });
    
    public Command ClearCountryFilterCommand => new(_ => CountryFilter = string.Empty);
    
    public Command<string> SelectCommunityCommand => new(community =>
    {
        CommunityFilter = community;
        SelectedCommunityFilter = community;
        SelectedCommunities.Add(community);
        CommunitySuggestions.Clear();
    });
    
    public Command<string> DeselectCommunityCommand => new(community =>
    {
        SelectedCommunities.Remove(community);
    });
    
    public Command ClearCommunityFilterCommand => new(_ => CommunityFilter = string.Empty);
    
    public EventFiltersViewModel(ISearchService searchService, IFilterService filterService)
    {
        InitializeCountries();
        _searchService = searchService;
        _filterService = filterService;
        ApplyFiltersCommand = new Command(ExecuteApplyFilters);
        ClearFiltersCommand = new Command(ExecuteClearFilters);
        DeleteFiltersCommand = new Command(ExecuteDeleteFilters);
    }
    
    public bool IsTagFilterNotEmpty => !string.IsNullOrEmpty(TagFilter);
    public bool IsSpeakerFilterNotEmpty => !string.IsNullOrEmpty(SpeakerFilter);
    public bool IsCountryFilterNotEmpty => !string.IsNullOrEmpty(CountryFilter);
    public bool IsCommunityFilterNotEmpty => !string.IsNullOrEmpty(CommunityFilter);

    public string TagFilter
    {
        get => _tagFilter;
        set
        {
            if (_tagFilter == value) return;
            if (!string.IsNullOrWhiteSpace(SelectedTagFilter))
            {
                if (SelectedTagFilter != value)
                {
                    SelectedTagFilter = null;
                }
            }
            _tagFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsTagFilterNotEmpty));
            DebounceSearchTags();
        }
    }

    private string SelectedTagFilter { get; set; }

    public string SpeakerFilter
    {
        get => _speakerFilter;
        set
        {
            if (_speakerFilter == value) return;
            if (!string.IsNullOrWhiteSpace(SelectedSpeakerFilter))
            {
                if (SelectedSpeakerFilter != value)
                {
                    SelectedSpeakerFilter = null;
                }
            }
            _speakerFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSpeakerFilterNotEmpty));
            DebounceSearchSpeakers();
        }
    }
    
    private string SelectedSpeakerFilter { get; set; }

    public string CountryFilter
    {
        get => _countryFilter;
        set
        {
            if (_countryFilter == value) return;
            if (!string.IsNullOrWhiteSpace(SelectedCountryFilter))
            {
                if (SelectedCountryFilter != value)
                {
                    SelectedCountryFilter = null;
                }
            }
            _countryFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCountryFilterNotEmpty));
            DebounceSearchCountries();
        }
    }
    
    private string SelectedCountryFilter { get; set; }

    public string CommunityFilter
    {
        get => _communityFilter;
        set
        {
            if (_communityFilter == value) return;
            if (!string.IsNullOrWhiteSpace(SelectedCommunityFilter))
            {
                if (SelectedCommunityFilter != value)
                {
                    SelectedCommunityFilter = null;
                }
            }
            _communityFilter = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCommunityFilterNotEmpty));
            DebounceSearchCommunities();
        }
    }
    
    private string SelectedCommunityFilter { get; set; }

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

    private void InitializeCountries()
    {
        Countries =
        [
            "United States",
            "Canada",
            "United Kingdom",
            "Germany",
            "France",
            "Spain",
            "Italy",
            "Russia",
            "China",
            "Japan",
            "India",
            "Brazil",
            "Australia"
        ];
    }

    private async void ExecuteApplyFilters()
    {
        try
        {
            var filters = new FilterOptions
            {
                TagFilters = SelectedTags.ToList(),
                SpeakerFilters = SelectedSpeakers.ToList(),
                CountryFilters = SelectedCountries.ToList(),
                CommunityFilters = SelectedCommunities.ToList(),
                OnlineOnly = OnlineOnly
            };

            ApplyFilters(filters);

            await Toast.Make("Filters Applied").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't applied: {e}").Show();
        }
    }

    private async void ExecuteClearFilters()
    {
        try
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
        
            await Toast.Make("Filters Cleared").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't cleared: {e}").Show();
        }
    }
    
    private async void ExecuteDeleteFilters()
    {
        try
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
        
            DeleteFilters();

            await Toast.Make("Filters Deleted").Show();
        }
        catch (Exception e)
        {
            await Toast.Make($"Filters weren't deleted: {e}").Show();
        }
    }

    private void ApplyFilters(FilterOptions filters)
    {
        _filterService.SetFilters(filters);
    }
    
    private void DebounceSearchTags()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token); // debounce delay
                if (!token.IsCancellationRequested)
                {
                    await SearchTags();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }
    
    private void DebounceSearchSpeakers()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token); // debounce delay
                if (!token.IsCancellationRequested)
                {
                    await SearchSpeakers();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    private void DebounceSearchCountries()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                {
                    await SearchCountries();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }
    
    private void DebounceSearchCommunities()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token); // debounce delay
                if (!token.IsCancellationRequested)
                {
                    await SearchCommunities();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    private async Task SearchTags()
    {
        if (!string.IsNullOrWhiteSpace(SelectedTagFilter) || string.IsNullOrWhiteSpace(TagFilter))
        {
            MainThread.BeginInvokeOnMainThread(() => TagSuggestions.Clear());
            return;
        }

        var tags = await _searchService.GetTags(TagFilter);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            TagSuggestions.Clear();
            foreach (var tag in tags)
                TagSuggestions.Add(tag);
        });
    }
    
    private async Task SearchSpeakers()
    {
        if (!string.IsNullOrWhiteSpace(SelectedSpeakerFilter) || string.IsNullOrWhiteSpace(SpeakerFilter))
        {
            MainThread.BeginInvokeOnMainThread(() => SpeakerSuggestions.Clear());
            return;
        }

        var speakers = await _searchService.GetSpeakers(SpeakerFilter);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            SpeakerSuggestions.Clear();
            foreach (var speaker in speakers)
                SpeakerSuggestions.Add(speaker);
        });
    }

    private Task SearchCountries()
    {
        if (!string.IsNullOrWhiteSpace(SelectedCountryFilter) || string.IsNullOrWhiteSpace(CountryFilter))
        {
            MainThread.BeginInvokeOnMainThread(() => CountrySuggestions.Clear());
            return Task.CompletedTask;
        }

        var filteredCountries = Countries
            .Where(c => c.Contains(CountryFilter, StringComparison.CurrentCultureIgnoreCase) && !SelectedCountries.Contains(c))
            .ToList();

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
        if (!string.IsNullOrWhiteSpace(SelectedCommunityFilter) || string.IsNullOrWhiteSpace(CommunityFilter))
        {
            MainThread.BeginInvokeOnMainThread(() => CommunitySuggestions.Clear());
            return;
        }

        var communities = await _searchService.GetCommunities(CommunityFilter);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CommunitySuggestions.Clear();
            foreach (var community in communities)
                CommunitySuggestions.Add(community);
        });
    }
    
    public void LoadFilters()
    {
        var filters = _filterService.GetFilters();
        if (filters == null) return;
        
        if (filters.TagFilters.Count != 0) SelectedTags = filters.TagFilters.ToObservableHashSet();
        if (filters.SpeakerFilters.Count != 0) SelectedSpeakers = filters.SpeakerFilters.ToObservableHashSet();
        if (filters.CountryFilters.Count != 0) SelectedCountries = filters.CountryFilters.ToObservableHashSet();
        if (filters.CommunityFilters.Count != 0) SelectedCommunities = filters.CommunityFilters.ToObservableHashSet();
        if (filters.OnlineOnly) OnlineOnly = true;
    }

    private void DeleteFilters()
    {
        _filterService.ClearFilters();
    }
}