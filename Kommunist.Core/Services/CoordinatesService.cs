using Kommunist.Core.Models;
using Kommunist.Core.Services.Interfaces;
using Newtonsoft.Json;

namespace Kommunist.Core.Services;

public class CoordinatesService : ICoordinatesService
{
    public async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return (0, 0);
        }
        using var httpClient = new HttpClient();
        var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(location)}";

        httpClient.DefaultRequestHeaders.Add("User-Agent", "MyIcsApp/1.0");

        var json = await httpClient.GetStringAsync(url);
        var results = JsonConvert.DeserializeObject<List<NominatimResult>>(json);

        if (results is { Count: > 0 })
        {
            return (double.Parse(results[0].Lat, System.Globalization.CultureInfo.InvariantCulture),
                double.Parse(results[0].Lon, System.Globalization.CultureInfo.InvariantCulture));
        }

        throw new Exception("Wasn't able to get coordinates.");
    }
}