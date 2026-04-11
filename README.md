# WeatherApi

A caching weather API built with ASP.NET Core that fetches real-time weather data from Visual Crossing and caches results in Redis using the cache-aside pattern.

## Features

- Real-time weather data via Visual Crossing API
- Redis caching with 12-hour expiration (cache-aside pattern)
- Cache hit/miss tracking on every response (`fromCache: true/false`)
- Graceful error handling — cache failures never break requests
- Structured logging throughout
- Docker support with multi-stage build

## Architecture

When a request comes in for a city, the service first checks Redis for a cached result. If the data is found (cache hit), it's returned immediately with `fromCache: true`. If not (cache miss), the service fetches live data from the Visual Crossing API, stores it in Redis with a 12-hour expiration, and returns it to the caller.

## Endpoints
GET /weather/{city}   — returns weather data for a city
GET /health           — health check

Example response:
```json
{
  "city": "Cairo, Egypt",
  "temperature": 32.4,
  "feelsLike": 30.1,
  "conditions": "Clear",
  "humidity": 28.0,
  "description": "Clear conditions throughout the day.",
  "fromCache": false
}
```

## Getting started

Add your Visual Crossing API key as a user secret:

```bash
dotnet user-secrets set "WeatherApi:ApiKey" "your_key_here"
```

Then run:

```bash
dotnet run
```

## Docker

```bash
docker build -t weather-api .
docker run -e WeatherApi__ApiKey=your_key_here \
  -e ConnectionStrings__Redis=host.docker.internal:6379 \
  -p 8080:8080 weather-api
```

## Configuration

- `WeatherApi:ApiKey` — your Visual Crossing API key (use secrets, not appsettings)
- `ConnectionStrings:Redis` — Redis connection string (default: `localhost:6379`)
