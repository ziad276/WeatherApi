namespace WeatherApi.Models
{
    public class Weather
    {
        public int Id { get; set; }
        public string City { get; set; }
        public int Temperature { get; set; }
        public string Season { get; set; }
    }
}
