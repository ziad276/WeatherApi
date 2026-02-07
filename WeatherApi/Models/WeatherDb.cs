using Microsoft.EntityFrameworkCore;

namespace WeatherApi.Models
{
    public class WeatherDb: DbContext
    {
        public WeatherDb(DbContextOptions<WeatherDb> options)
        : base(options) { }

        public DbSet<Weather> Weathers => Set<Weather>();

    }
}
