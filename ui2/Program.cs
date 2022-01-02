using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stl.DependencyInjection;
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
var env = builder.HostEnvironment;

builder.Services.AddLogging(logging => {
    // logging.ClearProviders();

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
