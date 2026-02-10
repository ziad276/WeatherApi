namespace WeatherApi.Models
{
    public class WeatherResponse
    {
        public string ResolvedAddress { get; set; } = string.Empty;  
        public string Description { get; set; } = string.Empty;
        public CurrentConditions CurrentConditions { get; set; }
    }

    public class CurrentConditions
    {
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public double Humidity { get; set; } 
        public string Conditions { get; set; } = string.Empty;  
    }

    public class WeatherDto
    {
        public string City { get; set; } = string.Empty;
        public double Temperature { get; set; }  // Changed to double
        public double FeelsLike { get; set; }
        public string Conditions { get; set; } = string.Empty;
        public double Humidity { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool FromCache { get; set; }  // Changed to bool
    }
}