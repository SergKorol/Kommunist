using System.Net;
using FluentAssertions;
using Kommunist.Core.Services;

namespace Kommunist.Tests.Services;

public class CoordinatesServiceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetCoordinatesAsync_ReturnsZeroZero_WhenLocationIsNullOrEmpty(string? location)
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        var (lat, lon) = await sut.GetCoordinatesAsync(location);

        lat.Should().Be(0);
        lon.Should().Be(0);
    }

    [Fact]
    public async Task GetCoordinatesAsync_ReturnsParsedCoordinates_WhenApiReturnsOneResult()
    {
        var json = "[{\"lat\":\"48.8584\",\"lon\":\"2.2945\"}]";
        string? capturedUserAgent = null;

        var handler = new TestHandler((req, _) =>
        {
            capturedUserAgent = req.Headers.UserAgent.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        var (lat, lon) = await sut.GetCoordinatesAsync("Eiffel Tower");

        lat.Should().BeApproximately(48.8584, 1e-7);
        lon.Should().BeApproximately(2.2945, 1e-7);
        capturedUserAgent.Should().Contain("MyIcsApp/1.0");
    }

    [Fact]
    public async Task GetCoordinatesAsync_ReturnsFirstResult_WhenApiReturnsMultipleResults()
    {
        var json = "[" +
                   "{\"lat\":\"10.1\",\"lon\":\"20.2\"}," +
                   "{\"lat\":\"30.3\",\"lon\":\"40.4\"}" +
                   "]";
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        var (lat, lon) = await sut.GetCoordinatesAsync("Somewhere");

        lat.Should().Be(10.1);
        lon.Should().Be(20.2);
    }

    [Fact]
    public async Task GetCoordinatesAsync_Throws_WhenApiReturnsEmptyArray()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        Func<Task> act = () => sut.GetCoordinatesAsync("NoResults");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Wasn't able to get coordinates.");
    }

    [Fact]
    public async Task GetCoordinatesAsync_Throws_WhenApiReturnsNull()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        Func<Task> act = () => sut.GetCoordinatesAsync("NoResults");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Wasn't able to get coordinates.");
    }

    [Fact]
    public async Task GetCoordinatesAsync_ThrowsHttpRequestException_OnNonSuccessStatus()
    {
        var handler = new TestHandler((_, _) => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad request")
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        Func<Task> act = () => sut.GetCoordinatesAsync("X");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetCoordinatesAsync_EncodesQueryParameter()
    {
        string? requestedUrl = null;
        var handler = new TestHandler((req, _) =>
        {
            requestedUrl = req.RequestUri?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"lat\":\"1\",\"lon\":\"2\"}]")
            };
        });
        var client = new HttpClient(handler);
        var sut = new CoordinatesService(client);

        var location = "New York, NY";
        await sut.GetCoordinatesAsync(location);

        requestedUrl.Should().NotBeNullOrEmpty();

        if (requestedUrl != null)
        {
            var uri = new Uri(requestedUrl);
            var query = uri.Query.TrimStart('?');
            var qParam = query.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(p => p.StartsWith("q=", StringComparison.Ordinal))?
                .Substring(2);

            qParam.Should().NotBeNull("q parameter should be present in the request URL");

            if (qParam != null)
            {
                var decoded = Uri.UnescapeDataString(qParam).Replace("+", " ");
                decoded.Should().Be(location);
            }
        }

        requestedUrl.Should().Contain("format=json");
    }

    private sealed class TestHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _responder = responder ?? throw new ArgumentNullException(nameof(responder));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request, cancellationToken));
    }
}
