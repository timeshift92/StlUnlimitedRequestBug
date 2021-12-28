using Stl.Fusion;

namespace FusionHybrid.Abstractions;

public interface ICounterService
{
    [ComputeMethod]
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task IncrementAsync(CancellationToken cancellationToken = default);
}