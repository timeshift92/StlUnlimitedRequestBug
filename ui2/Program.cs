using Blazor.Extensions.Logging;
using FusionHybrid.Abstractions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.Fusion.Client;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;
using ui2;
using Stl.DependencyInjection;

Random random = new Random();
string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}


//Write("Enter SessionId to use: ");
//var sessionId = ReadLine()!.Trim();
var session = new Session(RandomString(16));

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var Env = builder.HostEnvironment;
builder.Services.AddLogging(logging => {
    // logging.ClearProviders();

    logging.AddBrowserConsole();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.SetMinimumLevel(LogLevel.Trace);

    if (Env.IsDevelopment()) {
        logging.AddDebug();
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
        logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
    }
});

//var session = new Session("1!@ER!F@#$GWASZGA");
builder.Services.AddSingleton(session);
//services.AddSingleton(new SessionFactory().CreateSession());


var baseUri = new Uri("http://localhost:5000");
var apiBaseUri = new Uri($"{baseUri}api/");

var fusion = builder.Services.AddFusion();
var fusionClient = fusion.AddRestEaseClient(
    (c, o) => {
        o.BaseUri = baseUri;
        o.IsLoggingEnabled = true;
        o.IsMessageLoggingEnabled = true;
    }).ConfigureHttpClientFactory(
    (c, name, o) => {
        var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
        var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
        o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);

    });

fusionClient.ConfigureWebSocketChannel((c, o) => {
    o.BaseUri = baseUri;
    o.IsLoggingEnabled = true;
    o.IsMessageLoggingEnabled = true;
});
fusionClient.AddReplicaService<ICounterService, ICounterClientDef>();
fusion.AddAuthentication().AddRestEaseClient();


fusion.AddFusionTime();
fusion.AddBackendStatus();
// Default update delay is 0.1s
builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.1));


//ConfigureServices(builder.Services, builder);

var host = builder.Build();

await host.Services.HostedServices().Start();
await builder.Build().RunAsync();

void ConfigureServices(IServiceCollection services, WebAssemblyHostBuilder builder)
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);

    var baseUri = new Uri("http://localhost:5000"); //  new Uri(builder.HostEnvironment.BaseAddress);
    var apiBaseUri = new Uri($"{baseUri}api/");

    // Fusion
    var fusion = services.AddFusion();

    // Fusion services
    var fusionClient = fusion.AddRestEaseClient(
        (c, o) => {
            o.BaseUri = baseUri;
            o.IsLoggingEnabled = true;
            o.IsMessageLoggingEnabled = true;
        })
        .ConfigureHttpClientFactory((c, name, o)
            => {
                var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
            });

    fusion.AddAuthentication().AddRestEaseClient();
    // Fusion service clients
    fusionClient.AddReplicaService<ICounterService, ICounterClientDef>();
    //fusionClient.AddReplicaService<IWeatherForecastService, IWeatherForecastClientDef>();

    // Default update delay is 0.1s
    services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.1));
    //ConfigureSharedServices(services);
}

void ConfigureSharedServices(IServiceCollection services)
{
    // Other UI-related services
    //var fusion = services.AddFusion();
    //fusion.AddBlazorUIServices().AddPublisher((c, o) => {

    //    var channelProvider = c.GetRequiredService<IChannelProvider>();
    //    channelProvider.CreateChannel(o.Id,c.GetRequiredService<CancellationToken>());

    //});
    //fusion.AddFusionTime();
    //fusion.AddBackendStatus<CustomBackendStatus>();
    //// We don't care about Sessions in this sample, but IBackendStatus
    //// service assumes it's there, so let's register a fake one
    // services.AddSingleton(new SessionFactory().CreateSession());


    //// Default update delay is set to min.
    //services.AddTransient<IUpdateDelayer>(_ => UpdateDelayer.MinDelay);
}