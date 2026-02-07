using Microsoft.EntityFrameworkCore;
using WeatherApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WeatherDb>(opt => opt.UseInMemoryDatabase("weatherApi"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/weather", async (WeatherDb db) =>
    await db.Weathers.ToListAsync());

app.MapGet("/weather/city/{city}", async (string city, WeatherDb db) =>
      await db.Weathers.Where(w => w.City == city).ToListAsync());


app.MapGet("/weather/season/{season}", async (string season, WeatherDb db) =>
    await db.Weathers
        .Where(w => w.Season == season)
        .ToListAsync());

app.Run();