using FusionHybrid.BlazorClient;
using FusionHybrid.Abstractions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.Fusion.Client;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");



builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001") });
builder.Services.AddBlazorise().AddBootstrapProviders().AddFontAwesomeIcons();


var baseUri = new Uri("http://localhost:5000");
var apiBaseUri = new Uri($"{baseUri}api/");

builder.Services.ConfigureAll<HttpClientFactoryOptions>(options => {
    // Replica Services construct HttpClients using IHttpClientFactory, so this is
    // the right way to make all HttpClients to have BaseAddress = apiBaseUri by default.
    options.HttpClientActions.Add(client => client.BaseAddress = apiBaseUri);
});
var fusion = builder.Services.AddFusion();


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
fusion.AddAuthentication().AddRestEaseClient().AddBlazor();

// Default update delay is 0.1s

fusion.AddBlazorUIServices();
fusion.AddFusionTime();
fusion.AddBackendStatus<FusionHybrid.Services.CustomBackendStatus>();
// We don't care about Sessions in this sample, but IBackendStatus
// service assumes it's there, so let's register a fake one
builder.Services.AddSingleton(new SessionFactory().CreateSession());
builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 1));

await builder.Build().RunAsync();
