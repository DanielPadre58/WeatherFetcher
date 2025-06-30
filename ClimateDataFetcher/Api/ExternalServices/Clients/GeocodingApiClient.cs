using System.Text.Json;
using ClimateDataFetcher.Api.Data;

namespace ClimateDataFetcher.Api.ExternalServices.Clients;

public class GeocodingApiClient(string apiKey)
{
    private static readonly HttpClient Client = new();

    public async Task<Coordinate> GetLongitudeAndLatitude(Location location)
    {
        if (!Client.DefaultRequestHeaders.Contains("X-Api-Key"))
            Client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var uri = new Uri($"https://api.api-ninjas.com/v1/geocoding?city={location.City}&country={location.Country}");

        try
        {
            var response = await Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<Coordinate>>(responseBody, options)[0];
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"HTTP Request failed: {e.Message}");
            throw;
        }
    }
}