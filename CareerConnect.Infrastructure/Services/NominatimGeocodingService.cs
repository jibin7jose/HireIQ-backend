using System.Text.Json;
using CareerConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareerConnect.Infrastructure.Services;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;

    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        // Nominatim requires a User-Agent
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CareerConnectJobPortal/1.0");
    }

    public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(location)}&format=json&limit=1";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                
                if (document.RootElement.ValueKind == JsonValueKind.Array && document.RootElement.GetArrayLength() > 0)
                {
                    var result = document.RootElement[0];
                    if (result.TryGetProperty("lat", out var latProp) && result.TryGetProperty("lon", out var lonProp))
                    {
                        if (double.TryParse(latProp.GetString(), out var lat) && double.TryParse(lonProp.GetString(), out var lon))
                        {
                            return (lat, lon);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to geocode location: {Location}", location);
        }

        return null;
    }
}
