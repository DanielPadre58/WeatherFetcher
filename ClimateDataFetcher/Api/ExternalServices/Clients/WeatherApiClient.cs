using System.Text.Json;
using ClimateDataFetcher.Api.Data;

namespace ClimateDataFetcher.Api.ExternalServices.Clients;

public class WeatherApiClient(string apiKey)
{
    private const int WeekDays = 7;
    
    private static readonly HttpClient Client = new HttpClient();

    public async Task<string> GetCurrentWeatherData(Coordinate coordinate)
    {
        var uri = new Uri(
            $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={coordinate.Latitude},{coordinate.Longitude}"
        );

        return await FetchData(uri);
    }

    public async Task<string> GetWeekForecastData(Coordinate coordinate)
    {
        var uri = new Uri(
            $"http://api.weatherapi.com/v1/forecast.json?key={apiKey}&q={coordinate.Latitude},{coordinate.Longitude}&days={WeekDays - 1}"
        );

        return await FetchData(uri);
    }

    private static async Task<string> FetchData(Uri uri)
    {
        try
        {
            var response = await Client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"HTTP Request failed: {e.Message}");
            throw;
        }
    }
}