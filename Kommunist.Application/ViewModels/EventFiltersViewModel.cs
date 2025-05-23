using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using Kommunist.Application.Models;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.ViewModels;

public class EventFiltersViewModel : BaseViewModel
{
    private readonly ISearchService _searchService;
    private string _tagFilter;
    private string _speakerFilter;
    private string _countryFilter;
    private string _communityFilter;
    private bool _onlineOnly;
    private int _selectedCountryIndex = -1;
    

    public ObservableCollection<string> Countries { get; private set; }
    public ObservableCollection<string> TagSearchResults { get; } = new();
    public ObservableCollection<string> TagSuggestions { get; } = new();

    private CancellationTokenSource _debounceCts;

    public ICommand ApplyFiltersCommand { get; private set; }
    public ICommand ClearFiltersCommand { get; private set; }
    
    public Command<string> SelectTagCommand => new Command<string>(tag =>
    {
        TagFilter = tag;
        TagSuggestions.Clear();
    });

    public EventFiltersViewModel(ISearchService searchService)
    {
        InitializeCountries();
        _searchService = searchService;
        ApplyFiltersCommand = new Command(ExecuteApplyFilters);
        ClearFiltersCommand = new Command(ExecuteClearFilters);
    }

    public string TagFilter
    {
        get => _tagFilter;
        set
        {
            if (_tagFilter == value) return;
            _tagFilter = value;
            OnPropertyChanged();
            DebounceSearchTags();
        }
    }

    public string SpeakerFilter
    {
        get => _speakerFilter;
        set
        {
            if (_speakerFilter != value)
            {
                _speakerFilter = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectedCountryIndex
    {
        get => _selectedCountryIndex;
        set
        {
            if (_selectedCountryIndex != value)
            {
                _selectedCountryIndex = value;
                if (_selectedCountryIndex >= 0 && _selectedCountryIndex < Countries.Count)
                {
                    _countryFilter = Countries[_selectedCountryIndex];
                }
                else
                {
                    _countryFilter = null;
                }

                OnPropertyChanged();
            }
        }
    }

    public string CountryFilter
    {
        get => _countryFilter;
        private set
        {
            if (_countryFilter != value)
            {
                _countryFilter = value;
                OnPropertyChanged();
            }
        }
    }

    public string CommunityFilter
    {
        get => _communityFilter;
        set
        {
            if (_communityFilter != value)
            {
                _communityFilter = value;
                OnPropertyChanged();
            }
        }
    }

    public bool OnlineOnly
    {
        get => _onlineOnly;
        set
        {
            if (_onlineOnly != value)
            {
                _onlineOnly = value;
                OnPropertyChanged();
            }
        }
    }

    private void InitializeCountries()
    {
        // Sample list of countries - replace with your actual data source
        Countries = new ObservableCollection<string>
        {
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
            // Add more countries as needed
        };
    }

    private async void ExecuteApplyFilters()
    {
        // Create a filter object with all the values
        var filters = new FilterOptions
        {
            TagFilter = TagFilter,
            SpeakerFilter = SpeakerFilter,
            CountryFilter = CountryFilter,
            CommunityFilter = CommunityFilter,
            OnlineOnly = OnlineOnly
        };

        // Apply the filters - in a real app, you would call your service here
        ApplyFilters(filters);

        // Show a confirmation message
        await Toast.Make("Filters Applied").Show();
        
    }



    private async void ExecuteClearFilters()
    {
        // Clear all filter properties
        TagFilter = string.Empty;
        SpeakerFilter = string.Empty;
        SelectedCountryIndex = -1;
        CommunityFilter = string.Empty;
        OnlineOnly = false;

        // Clear filters in your service if needed
        ClearFilters();

        // Show a confirmation message
        await Toast.Make("Filters Cleared").Show();
    }

    // Methods to be implemented with your actual filtering logic
    private void ApplyFilters(FilterOptions filters)
    {
        // Implement your actual filter application logic here
        // For example: FilterService.ApplyFilters(filters);
    }

    private void ClearFilters()
    {
        // Implement your actual filter clearing logic here
        // For example: FilterService.ClearFilters();
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


    private async Task SearchTags()
    {
        if (string.IsNullOrWhiteSpace(TagFilter))
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
}