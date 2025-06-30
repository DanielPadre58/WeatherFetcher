using ClimateDataFetcher.Api.Data.Repositories;
using ClimateDataFetcher.Api.ExternalServices;
using ClimateDataFetcher.Api.ExternalServices.Clients;
using ClimateDataFetcher.Api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IConnectionMultiplexer>(opt =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("DockerRedisConnection") ??
                                  throw new InvalidOperationException(
                                      "Missing configuration: database connection string"))
);
builder.Services.AddSingleton<GeocodingApiClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["ApiKeys:Geocoding"]
                 ?? throw new InvalidOperationException("Missing API key for Geocoding");
    return new GeocodingApiClient(apiKey);
});
builder.Services.AddSingleton<WeatherApiClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["ApiKeys:WeatherApi"]
                 ?? throw new InvalidOperationException("Missing API key for WeatherApi");
    return new WeatherApiClient(apiKey);
});
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddSingleton<WeatherCacheRepo>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();