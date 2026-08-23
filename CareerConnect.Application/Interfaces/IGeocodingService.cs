namespace CareerConnect.Application.Interfaces;

public interface IGeocodingService
{
    Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string location);
}
