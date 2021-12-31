using FusionHybrid.Abstractions;
using Stl.Fusion;

namespace FusionHybrid.Services;
public class CounterService : ICounterService
{
    private volatile int _count;
    public virtual Task<int> GetCount(CancellationToken cancellationToken) => Task.FromResult(_count);

    public Task Increment(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _count);
        using (Computed.Invalidate())
            GetCount(cancellationToken);
        return Task.CompletedTask;
    }


}
