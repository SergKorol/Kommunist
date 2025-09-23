using System.Net;
using System.Text;
using FluentAssertions;
using Kommunist.Core.Services;

namespace Kommunist.Tests.Services;

public class SearchServiceTests
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
    public async Task GetTags_BuildsCorrectUrl_AndDeserializesResults()
    {
        // Arrange
        const string json = """["tag1","tag2"]""";
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }),
            out var handler
        );

        var sut = new SearchService(httpClient);

        // Act
        var result = (await sut.GetTags("cloud")).ToList();

        // Assert
        handler.Requests.Should().ContainSingle();
        var request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri?.ToString()
            .Should().Be("https://example.com/api/v2/dictionaries/skills/search?search_query=cloud");

        result.Should().BeEquivalentTo("tag1", "tag2");
    }

    [Fact]
    public async Task GetSpeakers_BuildsCorrectUrl_AndReturnsEmptyOnNullJson()
    {
        // Arrange
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            }),
            out var handler
        );

        var sut = new SearchService(httpClient);

        // Act
        var result = (await sut.GetSpeakers("john")).ToList();

        // Assert
        handler.Requests.Should().ContainSingle();
        handler.Requests.Single().RequestUri?.ToString()
            .Should().Be("https://example.com/api/v2/speakers/search?search_query=john");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCommunities_ReturnsEmpty_OnNonSuccessStatus()
    {
        // Arrange
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            out var handler
        );

        var sut = new SearchService(httpClient);

        // Act
        var result = (await sut.GetCommunities("devs")).ToList();

        // Assert
        handler.Requests.Should().ContainSingle();
        handler.Requests.Single().RequestUri?.ToString()
            .Should().Be("https://example.com/api/v2/communities/search?search_query=devs");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTags_ReturnsEmpty_OnInvalidJson()
    {
        // Arrange
        var httpClient = CreateHttpClient(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{bad-json", Encoding.UTF8, "application/json")
            }),
            out _
        );

        var sut = new SearchService(httpClient);

        // Act
        var result = (await sut.GetTags("x")).ToList();

        // Assert
        result.Should().BeEmpty("invalid JSON should be caught and return empty sequence");
    }

    [Fact]
    public async Task GetSpeakers_ReturnsEmpty_OnHandlerException()
    {
        // Arrange
        var httpClient = CreateHttpClient(
            _ => throw new HttpRequestException("Network failure"),
            out _
        );

        var sut = new SearchService(httpClient);

        // Act
        var result = (await sut.GetSpeakers("any")).ToList();

        // Assert
        result.Should().BeEmpty("any exception should be caught and return empty sequence");
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
