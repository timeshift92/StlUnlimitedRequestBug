using BlazorServeClient.Data;
using FusionHybrid.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Stl.Fusion.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
var baseUri = new Uri("https://localhost:5001");
var apiBaseUri = new Uri($"{baseUri}api/");

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

fusion.AddFusionTime();
fusion.AddBackendStatus<FusionHybrid.Services.CustomBackendStatus>();
// We don't care about Sessions in this sample, but IBackendStatus
// service assumes it's there, so let's register a fake one
builder.Services.AddSingleton(new SessionFactory().CreateSession());
builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UICommandTracker(), 0.1));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
