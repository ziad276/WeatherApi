using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public class WeatherService(HttpClient httpClient,
         IDistributedCache cache,
         IConfiguration configuration,
         ILogger<WeatherService> logger)
    {
        
        private const int CACHE_EXPIRATION_SECONDS = 43200;

         
        public async Task<WeatherDto?> GetWeatherAsync(string city)
        {
            string cacheKey = $"weather:{city.ToLower()}";
            var cachedWeather = await GetFromCacheAsync(cacheKey);

            if (cachedWeather != null)
            {
                logger.LogInformation("Cache hit for city: {City}", city);
                cachedWeather.FromCache = true;
                return cachedWeather;
            }

            logger.LogInformation("Cache miss for city: {City}. Fetching from API.", city);

            var weatherResponse = await FetchWeatherFromApiAsync(city);

            if (weatherResponse is null)
            {
                return null;   
            }

            var weatherDto = MapToDto(weatherResponse);
            await SaveToCacheAsync(cacheKey, weatherDto);
            weatherDto.FromCache = false;
            return weatherDto;
        }

        private WeatherDto MapToDto(WeatherResponse weatherResponse)
        {
            return new WeatherDto
            {
                City = weatherResponse.ResolvedAddress,
                Temperature = weatherResponse.CurrentConditions?.Temp?? 0,
                FeelsLike = weatherResponse.CurrentConditions?.FeelsLike?? 0,
                Conditions = weatherResponse.CurrentConditions?.Conditions?? "Unknown",
                Humidity = weatherResponse.CurrentConditions?.Humidity?? 0,
                Description = weatherResponse.Description?? "No description available",

            };
        }

        private async Task<WeatherResponse?> FetchWeatherFromApiAsync(string city)
        {
            
            try
            {
                var apiKey = configuration["WeatherApi:ApiKey"];
                if(string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("API key for weather service is not configured.");
                }

                var url = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/" +
                    $"{Uri.EscapeDataString(city)}?key={apiKey}&unitGroup=metric";
                logger.LogInformation("Fetching weather data from API for city: {City} using URL: {Url}", city, url);

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Failed to fetch weather data from API for city: {City}. Status Code: {StatusCode}", city, response.StatusCode);
                    return null;
                }
               var content = await response.Content.ReadAsStringAsync();

               return JsonSerializer.Deserialize<WeatherResponse>(content, new JsonSerializerOptions{
                    PropertyNameCaseInsensitive = true
                });

            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error fetching weather for city: {City}", city);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error fetching weather for city: {City}", city);
                throw;
            }
        }



        private async Task<WeatherDto?> GetFromCacheAsync(string cacheKey)
        {
            try
            {
                var cacheData = await cache.GetStringAsync(cacheKey);

                if (string.IsNullOrWhiteSpace(cacheData))
                {
                    
                    return null;
                }
                var weather = JsonSerializer.Deserialize<WeatherDto>(cacheData);
                return weather;

            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving weather data from cache for key: {CacheKey}", cacheKey);
                return null;
            }
        }
        private async Task SaveToCacheAsync(string cacheKey, WeatherDto weatherDto) // saving the weatherdto to cache
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(weatherDto);
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_EXPIRATION_SECONDS)
                };
                await cache.SetStringAsync(cacheKey, jsonData, cacheOptions);
                logger.LogInformation("Weather data cached for key: {CacheKey} with expiration of {ExpirationSeconds} seconds", cacheKey, CACHE_EXPIRATION_SECONDS);
            }
            catch (Exception ex)
            {

                logger.LogWarning(ex, "Failed to save weather data to cache for key: {CacheKey}. Cache will be bypassed for this entry.", cacheKey);
            }

        }
    };

    
}
