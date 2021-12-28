namespace FusionHybrid.Abstractions;

public interface IWeatherForecastService
{
    [ComputeMethod]
    Task<WeatherForecast[]> GetForecast(DateTime startDate, CancellationToken cancellationToken = default);
}
