using StackExchange.Redis;

namespace ClimateDataFetcher.Api.Data.Repositories;

public class WeatherCacheRepo
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _redis;

    public WeatherCacheRepo(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async void PutWeatherData(string dataKey, string data)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentNullException(nameof(data), "data cannot be null or empty");

        if (string.IsNullOrEmpty(dataKey))
            throw new ArgumentNullException(nameof(dataKey), "data key cannot be null or empty");

        await _db.StringSetAsync(dataKey, data, TimeSpan.FromMinutes(10));
    }

    public async Task<string?> GetWeatherData(string dataKey)
    {
        if (string.IsNullOrEmpty(dataKey))
            throw new ArgumentNullException(nameof(dataKey));

        await _db.KeyExpireAsync(dataKey, TimeSpan.FromMinutes(10));

        return await _db.StringGetAsync(dataKey);
    }
}