using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class CoordinatesService(HttpClient httpClient) : ICoordinatesService
{
    private readonly HttpClient? _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string? location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return (0, 0);
        }

        var client = _httpClient ?? new HttpClient();
        var disposeClient = _httpClient is null;

        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(location)}";

            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MyIcsApp/1.0");
            }

            var json = await client.GetStringAsync(url);
            var results = JsonConvert.DeserializeObject<List<NominatimResult>>(json);

            if (results is not { Count: > 0 }) throw new Exception("Wasn't able to get coordinates.");
            var lat = results[0].Lat;
            if (lat == null) throw new Exception("Wasn't able to get coordinates.");
            var lon = results[0].Lon;
            if (lon != null)
                return (double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture),
                    double.Parse(lon, System.Globalization.CultureInfo.InvariantCulture));

            throw new Exception("Wasn't able to get coordinates.");
        }
        finally
        {
            if (disposeClient)
            {
                client.Dispose();
            }
        }
    }
}