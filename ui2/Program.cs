using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.DependencyInjection;
using ui2;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var env = builder.HostEnvironment;

builder.Services.AddLogging(logging => {
    logging.ClearProviders();
    logging.AddBrowserConsole();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.SetMinimumLevel(LogLevel.Information);
    if (env.IsDevelopment()) {
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
        logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
    }
});

UIStartup.ConfigureServices(builder.Services, builder);
var host = builder.Build();

await host.Services.HostedServices().Start();
await builder.Build().RunAsync();
