namespace Kommunist.Core.Services.Interfaces;

public interface ICoordinatesService
{
    Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string? location);
}