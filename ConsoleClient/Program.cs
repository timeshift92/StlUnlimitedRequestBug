using FusionHybrid.Abstractions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.UI;
using static System.Console;

Write("Enter SessionId to use: ");
var sessionId = ReadLine()!.Trim();
var session = new Session(sessionId);

var services = CreateServiceProvider();



var counterService = services.GetRequiredService<ICounterService>();
var computed = await Computed.Capture(ct => counterService.GetCount());
while (true) {
    WriteLine($"- {computed.Value}");
    await computed.WhenInvalidated();
    computed = await computed.Update();
}

IServiceProvider CreateServiceProvider()
{
    // ReSharper disable once VariableHidesOuterVariable
    var services = new ServiceCollection();
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var builder = new ConfigurationBuilder()
        .AddJsonFile($"appsettings.json", true, true)
        .AddJsonFile($"appsettings.{environmentName}.json", true, true)
        .AddEnvironmentVariables();
    var configuration = builder.Build();

    services.AddLogging(logging => {
        logging.ClearProviders();
        logging.AddConfiguration(configuration.GetSection("Logging"));
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddConsole();
    });

    var baseUri = new Uri("https://localhost:5001");
    var apiBaseUri = new Uri($"{baseUri}api/");

    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient(
        (c, o) => {
            o.BaseUri = baseUri;
        }).ConfigureHttpClientFactory(
        (c, name, o) => {
            var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
        }).ConfigureWebSocketChannel((c, o) => {
            o.BaseUri = baseUri;
            o.IsLoggingEnabled = true;
        });
    fusionClient.AddReplicaService<ICounterService, ICounterClientDef>();
    //fusion.AddAuthentication().AddRestEaseClient();

    // Default update delay is 0.1s
    services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.1));

    return services.BuildServiceProvider();
}

public static class CorsPolicyBuilderExt
{
    public static CorsPolicyBuilder WithFusionHeaders(this CorsPolicyBuilder builder)
        => builder.WithExposedHeaders(
            FusionHeaders.RequestPublication,
            FusionHeaders.Publication
        );
}
