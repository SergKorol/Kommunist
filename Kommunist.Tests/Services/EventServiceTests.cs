using System.Net;
using System.Text;
using FluentAssertions;
using Kommunist.Core.ApiModels;
using Kommunist.Core.Models;
using Moq;
using Newtonsoft.Json;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Tests.Services;

public class EventServiceTests
{
    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder, out TestHttpMessageHandler handler)
    {
        handler = new TestHttpMessageHandler(responder);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com")
        };
    }

    [Fact]
    public async Task LoadEvents_BuildsUrlWithAllFilters_AndReturnsEmptyOnEmptyArray()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 2);
        var endDate = new DateTime(2025, 1, 31);
        var expectedFrom = Uri.EscapeDataString(startDate.ToString("MM/dd/yyyy")).Replace(".", "%2F");
        var expectedTo = Uri.EscapeDataString(endDate.ToString("MM/dd/yyyy")).Replace(".", "%2F");

        var filterOptions = new FilterOptions
        {
            TagFilters = ["tag1", "tag2"],
            SpeakerFilters = ["sp1"],
            CountryFilters = ["US", "DE"],
            CommunityFilters = ["comm"],
            OnlineOnly = true
        };

        var filterService = new Mock<IFilterService>();
        filterService.Setup(x => x.GetFilters()).Returns(filterOptions);

        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            }),
            out var handler
        );

        var sut = new EventService(httpClient, filterService.Object);

        // Act
        var result = await sut.LoadEvents(startDate, endDate);

        // Assert
        var request = handler.Requests.Should().ContainSingle().Subject;
        var uri = request.RequestUri?.ToString();

        uri.Should().StartWith($"https://example.com/api/v2/calendar?start_date={expectedFrom}&end_date={expectedTo}");

        uri.Should().Contain("&tag[]=tag1")
           .And.Contain("&tag[]=tag2")
           .And.Contain("&speaker[]=sp1")
           .And.Contain("&location[]=US")
           .And.Contain("&location[]=DE")
           .And.Contain("&community[]=comm")
           .And.Contain("&online=Online");

        result.Should().BeEmpty("empty JSON array should deserialize to an empty sequence");
    }

    [Fact]
    public async Task LoadEvents_DeserializesResponse_WhenSuccess()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        filterService.Setup(x => x.GetFilters()).Returns(new FilterOptions());

        var events = new[]
        {
            new ServiceEvent { Id = 42, Title = "Test Event", EventUrl = "https://evt" }
        };

        var json = JsonConvert.SerializeObject(events);

        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }),
            out _
        );

        var sut = new EventService(httpClient, filterService.Object);

        // Act
        var result = (await sut.LoadEvents(DateTime.UtcNow.Date, DateTime.UtcNow.Date) ?? []).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(42);
        result[0].Title.Should().Be("Test Event");
    }

    [Fact]
    public async Task LoadEvents_ReturnsEmpty_WhenHttpRequestFails()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        filterService.Setup(x => x.GetFilters()).Returns(new FilterOptions());

        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            out _
        );

        var sut = new EventService(httpClient, filterService.Object);

        // Act
        var result = await sut.LoadEvents(DateTime.UtcNow.Date, DateTime.UtcNow.Date);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task LoadEvents_DoesNotAppendFilterParams_WhenFiltersAreEmpty()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        filterService.Setup(x => x.GetFilters()).Returns(new FilterOptions()); // all lists empty, OnlineOnly false

        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            }),
            out var handler
        );

        var sut = new EventService(httpClient, filterService.Object);

        var startDate = new DateTime(2025, 2, 10);
        var endDate = new DateTime(2025, 2, 20);
        var expectedFrom = Uri.EscapeDataString(startDate.ToString("MM/dd/yyyy")).Replace(".", "%2F");
        var expectedTo = Uri.EscapeDataString(endDate.ToString("MM/dd/yyyy")).Replace(".", "%2F");

        // Act
        await sut.LoadEvents(startDate, endDate);

        // Assert
        var uri = handler.Requests.Single().RequestUri?.ToString();
        uri.Should().Be($"https://example.com/api/v2/calendar?start_date={expectedFrom}&end_date={expectedTo}");
    }

    [Fact]
    public async Task GetHomePage_ReturnsEmpty_WhenHttpRequestFails_AndCallsCorrectUrl()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            out var handler
        );
        var sut = new EventService(httpClient, filterService.Object);
        const int eventId = 123;

        // Act
        var result = await sut.GetHomePage(eventId);

        // Assert
        handler.Requests.Should().ContainSingle();
        handler.Requests.Single().RequestUri?.ToString()
            .Should().Be("https://example.com/api/v2/events/123/pages/home");
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetHomePage_ReturnsEmpty_WhenApiReturnsEmptyArray()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            }),
            out _
        );
        var sut = new EventService(httpClient, filterService.Object);

        // Act
        var result = await sut.GetHomePage(77);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAgenda_ReturnsNull_WhenHttpRequestFails_AndCallsCorrectUrl()
    {
        // Arrange
        var filterService = new Mock<IFilterService>();
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            out var handler
        );
        var sut = new EventService(httpClient, filterService.Object);

        // Act
        var result = await sut.GetAgenda(55);

        // Assert
        handler.Requests.Should().ContainSingle();
        handler.Requests.Single().RequestUri?.ToString()
            .Should().Be("https://example.com/api/v2/events/55/agenda");
        result.Should().BeNull();
    }

    private sealed class TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return _handler(request);
        }
    }
}
