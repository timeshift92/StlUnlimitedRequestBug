using Stl.Fusion;

namespace FusionHybrid.Abstractions;

public interface ICounterService
{
    [ComputeMethod]
    Task<int> GetCount(CancellationToken cancellationToken = default);
    Task Increment(CancellationToken cancellationToken = default);
}