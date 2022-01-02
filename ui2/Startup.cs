using FusionHybrid.Abstractions;
using FusionHybrid.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;

namespace ui2;

public static class UIStartup
{
    public static void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
    {
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        var baseUri = new Uri("https://localhost:5001");
        var apiBaseUri = new Uri($"{baseUri}api/");

        // Fusion

        var fusion = services.AddFusion();
        fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use
        services.AddScoped<BlazorModeHelper>();

        // Fusion services
        var fusionClient = fusion.AddRestEaseClient(
            (c, o) => {
                o.BaseUri = baseUri;
                o.IsLoggingEnabled = true;
                o.IsMessageLoggingEnabled = false;

            }).ConfigureHttpClientFactory(
            (c, name, o) => {
                var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);

            });
        services.AddScoped<BlazorModeHelper>();


        // Fusion service clients
        fusionClient.AddReplicaService<ICounterService, ICounterClientDef>();
        fusionClient.AddReplicaService<IWeatherForecastService, IWeatherForecastClientDef>();

        ConfigureSharedServices(services);
    }

    public static void ConfigureSharedServices(IServiceCollection services)
    {

        // Other UI-related services
        var fusion = services.AddFusion();
        fusion.AddBlazorUIServices().AddPublisher();
        fusion.AddFusionTime();
        fusion.AddBackendStatus<CustomBackendStatus>();
        // We don't care about Sessions in this sample, but IBackendStatus
        // service assumes it's there, so let's register a fake one
        services.AddSingleton(new SessionFactory().CreateSession());

        // Default update delay is set to min.
        services.AddTransient<IUpdateDelayer>(_ => UpdateDelayer.MinDelay);
    }
}
