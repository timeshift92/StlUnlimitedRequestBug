using FusionHybrid.Abstractions;
using Stl.Fusion.Extensions;

namespace FusionHybrid.Services;
public class CustomBackendStatus : BackendStatus
{
    private readonly ICounterService _counterService;
    private readonly ILogger _log;

    public CustomBackendStatus(ICounterService counterService, ILogger<CustomBackendStatus> log)
        : base(null!)
    {
        _log = log;
        _counterService = counterService;
    }

    [ComputeMethod]
    protected override async Task<Unit> HitBackend(
        Session session,
        string backend,
        CancellationToken cancellationToken = default)
    {
        await _counterService.GetCount(cancellationToken);
        return default;
    }
}