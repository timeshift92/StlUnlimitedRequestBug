using RestEase;

namespace FusionHybrid.Abstractions;

[BasePath("Counter")]
public interface ICounterClientDef
{
    [Post("Increment")]
    Task Increment(CancellationToken cancellationToken = default);

    [Get("GetCount")]
    Task<int> GetCount(CancellationToken cancellationToken = default);


}

[BasePath("WeatherForecast")]
public interface IWeatherForecastClientDef
{
    [Get("GetForecast")]
    Task<WeatherForecast[]> GetForecast(DateTime startDate, CancellationToken cancellationToken = default);
}