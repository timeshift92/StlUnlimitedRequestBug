using FusionHybrid.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;

namespace FusionHybrid.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController, JsonifyErrors]
public class CounterController : ControllerBase,ICounterService
{
    private readonly ICounterService _counterService;

    public CounterController(ICounterService counterService) => _counterService = counterService;
    

    [HttpGet,Publish]
    public async Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        return await _counterService.GetCountAsync(cancellationToken);
    }

    [HttpPost,Publish]
    public async Task IncrementAsync(CancellationToken cancellationToken)
    {
        await _counterService.IncrementAsync(cancellationToken);
    }
}