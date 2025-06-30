using ClimateDataFetcher.Api.Data;
using ClimateDataFetcher.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClimateDataFetcher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetWeatherData([FromQuery] Location location, [FromQuery] bool week)
    {
        if (string.IsNullOrEmpty(location.Country))
            return BadRequest("Country and city are required");

        string weatherData;

        if (week)
            weatherData = await _weatherService.GetWeekForecastData(location);
        else
            weatherData = await _weatherService.GetWeatherData(location);

        return string.IsNullOrEmpty(weatherData) ? NotFound() : Ok(weatherData);
    }
}