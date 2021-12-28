using FusionHybrid.Abstractions;
using Stl.Fusion;

namespace FusionHybrid.Services;
public class CounterService : ICounterService
{
    private volatile int _count;
    public virtual Task<int> GetCountAsync(CancellationToken cancellationToken) => Task.FromResult(_count);

    public Task IncrementAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _count);
        using (Computed.Invalidate())
            GetCountAsync(cancellationToken);
        return Task.CompletedTask;
    }


}
