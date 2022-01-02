using FusionHybrid.Server;
using Microsoft.Extensions.Configuration.Memory;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(cfg => {
       // Looks like there is no better way to set _default_ URL
       cfg.Sources.Insert(0, new MemoryConfigurationSource() {
           InitialData = new Dictionary<string, string>() {
               {WebHostDefaults.ServerUrlsKey, "https://localhost:7245;https://localhost:5001"},
           }
       });
    })
    .ConfigureWebHostDefaults(webHost => webHost
        .UseDefaultServiceProvider((ctx, options) => {
            options.ValidateScopes = ctx.HostingEnvironment.IsDevelopment();
            options.ValidateOnBuild = true;
        })
        .UseStartup<Startup>())
    .Build();

await host.RunAsync();
