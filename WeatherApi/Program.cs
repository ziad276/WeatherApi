using Microsoft.Extensions.Caching.Distributed;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHttpClient<WeatherService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddScoped<WeatherService>();

var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

// Main weather endpoint
app.MapGet("/weather/{city}", async (string city, WeatherService weatherService) =>
{
    try
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(city))
        {
            return Results.BadRequest(new { error = "City name is required" });
        }

        // Get weather from service
        var weather = await weatherService.GetWeatherAsync(city);

        // Check if found
        if (weather == null)
        {
            return Results.NotFound(new { error = $"Weather data not found for city: {city}" });
        }

        // Return success
        return Results.Ok(weather);
    }
    catch (InvalidOperationException ex)
    {
        // Configuration error (missing API key)
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
    catch (HttpRequestException)
    {
        // External API is down
        return Results.Problem(
            detail: "Weather service is currently unavailable. Please try again later.",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
    catch (Exception)
    {
        // Unexpected error
        return Results.Problem(
            detail: "An unexpected error occurred",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});

app.Run();