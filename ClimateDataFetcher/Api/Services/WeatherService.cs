using System.Text.Json;
using System.Text.Json.Nodes;
using ClimateDataFetcher.Api.Data;
using ClimateDataFetcher.Api.Data.Repositories;
using ClimateDataFetcher.Api.ExternalServices.Clients;

namespace ClimateDataFetcher.Api.Services;

public class WeatherService(
    GeocodingApiClient geocodingApiClient,
    WeatherApiClient weatherApiClient,
    WeatherCacheRepo weatherCacheRepo)
{
    public async Task<string> GetWeatherData(Location location)
    {
        if (string.IsNullOrEmpty(location.City))
            throw new InvalidOperationException("Missing Parameters: 'City'");

        if (string.IsNullOrEmpty(location.Country))
            throw new InvalidOperationException("Missing Parameters: 'Country'");

        var cacheData = weatherCacheRepo.GetWeatherData(GetDataKey(location)).Result;

        if (cacheData != null)
            return cacheData;

        try
        {
            var coordinates = await geocodingApiClient.GetLongitudeAndLatitude(location);

            var weatherDataJson = await weatherApiClient.GetCurrentWeatherData(coordinates);

            weatherCacheRepo.PutWeatherData(GetDataKey(location), weatherDataJson);

            return weatherDataJson;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong fetching weather data");
        }

        return string.Empty;
    }
    
    public async Task<string> GetWeekForecastData(Location location)
    {
        if (string.IsNullOrEmpty(location.City))
            throw new InvalidOperationException("Missing Parameters: 'City'");

        if (string.IsNullOrEmpty(location.Country))
            throw new InvalidOperationException("Missing Parameters: 'Country'");

        var cacheData = weatherCacheRepo.GetWeatherData($"{GetDataKey(location)}:{DateTime.Today}").Result;

        if (cacheData != null)
            return cacheData;

        try
        {
            var coordinates = await geocodingApiClient.GetLongitudeAndLatitude(location);
            var weatherDataJson = await weatherApiClient.GetWeekForecastData(coordinates);

            var root = JsonNode.Parse(weatherDataJson);
            var forecastDays = root?["forecast"]?["forecastday"]?.AsArray();

            if (forecastDays == null)
                return string.Empty;

            foreach (var day in forecastDays)
            {
                day?.AsObject()?.Remove("astro");
                day?.AsObject()?.Remove("hour");
                day?.AsObject()?.Remove("air_quality");
            }

            var filteredResponse = forecastDays.ToJsonString();

            weatherCacheRepo.PutWeatherData($"{GetDataKey(location)}:{DateTime.Today}", filteredResponse);

            return filteredResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong fetching weather data");
        }

        return string.Empty;
    }
    

    private string GetDataKey(Location location)
    {
        return $"{location.Country}:{location.City}";
    }
}