using RestEase;

[BasePath("counter")]
public interface ICounterClientDef
{
    [Post("IncrementAsync")]
    Task IncrementAsync(CancellationToken cancellationToken = default);

    [Get("GetCountAsync")]
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}