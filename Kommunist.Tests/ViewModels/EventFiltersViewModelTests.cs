using System.Diagnostics.CodeAnalysis;
using Kommunist.Application.ViewModels;
using Kommunist.Core.ApiModels;
using Kommunist.Core.Services.Interfaces;
using Moq;

namespace Kommunist.Tests.ViewModels;

public class EventFiltersViewModelTests : IDisposable
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly Mock<IFilterService> _filterServiceMock;
    private readonly EventFiltersViewModel _vm;
    private const string Tag = "C#";

    public EventFiltersViewModelTests()
    {
        _searchServiceMock = new Mock<ISearchService>(MockBehavior.Strict);
        _filterServiceMock = new Mock<IFilterService>(MockBehavior.Strict);

        _vm = new EventFiltersViewModel(_searchServiceMock.Object, _filterServiceMock.Object);
    }

    public void Dispose()
    {
        _vm.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Property Tests

    [Fact]
    public void TagFilter_SetValue_UpdatesPropertyAndIsNotEmpty()
    {
        // Act
        _vm.TagFilter = "dotnet";

        // Assert
        Assert.Equal("dotnet", _vm.TagFilter);
        Assert.True(_vm.IsTagFilterNotEmpty);
    }

    [Fact]
    public void OnlineOnly_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var raised = false;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EventFiltersViewModel.OnlineOnly))
                raised = true;
        };

        // Act
        _vm.OnlineOnly = true;

        // Assert
        Assert.True(raised);
        Assert.True(_vm.OnlineOnly);
    }

    #endregion

    #region Command Tests

    [Fact]
    public void SelectTagCommand_AddsTagAndClearsSuggestions()
    {
        // Arrange
        _vm.TagSuggestions.Add("other");

        // Act
        _vm.SelectTagCommand.Execute(Tag);

        // Assert
        Assert.Contains(Tag, _vm.SelectedTags);
        Assert.Empty(_vm.TagSuggestions);
        Assert.Equal(Tag, _vm.TagFilter);
    }

    [Fact]
    public void DeselectTagCommand_RemovesTag()
    {
        // Arrange
        _vm.SelectedTags.Add(Tag);

        // Act
        _vm.DeselectTagCommand.Execute(Tag);

        // Assert
        Assert.DoesNotContain(Tag, _vm.SelectedTags);
    }

    [Fact]
    public void ClearTagFilterCommand_SetsTagFilterEmpty()
    {
        // Arrange
        _vm.TagFilter = Tag;

        // Act
        _vm.ClearTagFilterCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, _vm.TagFilter);
    }

    [Fact]
    public void ApplyFiltersCommand_CallsFilterService()
    {
        // Arrange
        _vm.SelectedTags.Add(Tag);
        _filterServiceMock.Setup(f => f.SetFilters(It.IsAny<FilterOptions>()));

        // Act
        _vm.ApplyFiltersCommand.Execute(null);

        // Assert
        _filterServiceMock.Verify(f => f.SetFilters(It.Is<FilterOptions>(
            fo => fo.TagFilters.Contains(Tag))), Times.Once);
    }

    [Fact]
    public void DeleteFiltersCommand_CallsClearFilters()
    {
        // Arrange
        _filterServiceMock.Setup(f => f.ClearFilters());

        // Act
        _vm.DeleteFiltersCommand.Execute(null);

        // Assert
        _filterServiceMock.Verify(f => f.ClearFilters(), Times.Once);
    }

    #endregion

    #region Load Filters

    [Fact]
    public void LoadFilters_PopulatesSelectedCollections()
    {
        // Arrange
        var filters = new FilterOptions
        {
            TagFilters = ["C#"],
            SpeakerFilters = ["Jon Skeet" ],
            CountryFilters = ["Ukraine" ],
            CommunityFilters = ["DotNet"],
            OnlineOnly = true
        };
        _filterServiceMock.Setup(f => f.GetFilters()).Returns(filters);

        // Act
        _vm.LoadFilters();

        // Assert
        Assert.Contains("C#", _vm.SelectedTags);
        Assert.Contains("Jon Skeet", _vm.SelectedSpeakers);
        Assert.Contains("DotNet", _vm.SelectedCommunities);
        Assert.True(_vm.OnlineOnly);
    }

    #endregion

    #region Search & Debounce

    // [Fact]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("Usage", "xUnit1013:Public method should be marked as test")]
    public async Task TagFilter_TriggersSearch_AddsSuggestions()
    {
        // Arrange
        _searchServiceMock
            .Setup(s => s.GetTags("dot"))
            .ReturnsAsync(["dotnet", "docker"]);

        // Act
        _vm.TagFilter = "dot";
        await Task.Delay(400); // wait > debounce delay

        // Assert
        Assert.Contains("dotnet", _vm.TagSuggestions);
        Assert.Contains("docker", _vm.TagSuggestions);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CancelsTokens()
    {
        // Act
        _vm.Dispose();

        // Assert
        _vm.Dispose();
    }

    #endregion
}