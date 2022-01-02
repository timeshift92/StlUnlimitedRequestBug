using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using FusionHybrid.Abstractions;
using FusionHybrid.Services;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Fusion.Server;


namespace FusionHybrid.Server;

public class Startup
{
    private IConfiguration Cfg { get; }
    private IWebHostEnvironment Env { get; }
    private ILogger Log { get; set; } = NullLogger<Startup>.Instance;

    public Startup(IConfiguration cfg, IWebHostEnvironment environment)
    {
        Cfg = cfg;
        Env = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(logging => {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            if (Env.IsDevelopment()) {
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
            }
        });

        services.AddCors();

#pragma warning disable ASP0000
        var tmpServices = services.BuildServiceProvider();
#pragma warning restore ASP0000
        Log = tmpServices.GetRequiredService<ILogger<Startup>>();

        // Fusion
        var fusion = services.AddFusion();
        var fusionServer = fusion.AddWebServer();
        fusion.AddFusionTime(); // IFusionTime is one of built-in compute services you can use
        services.AddScoped<BlazorModeHelper>();

        // Fusion services
        fusion.AddComputeService<ICounterService, CounterService>();
        fusion.AddComputeService<IWeatherForecastService, WeatherForecastService>();

        // Shared UI services
        ui2.UIStartup.ConfigureSharedServices(services);

        // Web
        services.Configure<ForwardedHeadersOptions>(options => {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        services.AddRouting();
        services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());
        services.AddServerSideBlazor(o => o.DetailedErrors = true);

        // Swagger & debug tools
        services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "Samples.Blazor.Server API", Version = "v1"
            });
        });
    }

    public void Configure(IApplicationBuilder app, ILogger<Startup> log)
    {
        // This server serves static content from Blazor Client,
        // and since we don't copy it to local wwwroot,
        // we need to find Client's wwwroot in bin/(Debug/Release) folder
        // and set it as this server's content root.
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
        var binCfgPart = Regex.Match(baseDir, @"[\\/]bin[\\/]\w+[\\/]").Value;
        var wwwRootPath = Path.Combine(baseDir, "wwwroot");
        if (!Directory.Exists(Path.Combine(wwwRootPath, "_framework")))
            // This is a regular build, not a build produced w/ "publish",
            // so we remap wwwroot to the client's wwwroot folder
            wwwRootPath = Path.GetFullPath(Path.Combine(baseDir, $"../../../../ui2/{binCfgPart}/net6.0/wwwroot"));
        Env.WebRootPath = wwwRootPath;
        Env.WebRootFileProvider = new PhysicalFileProvider(Env.WebRootPath);
        StaticWebAssetsLoader.UseStaticWebAssets(Env, Cfg);

        if (Env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseForwardedHeaders(new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        app.UseWebSockets(new WebSocketOptions() {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });

        app.UseCors(c => {
            c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });

        // Static + Swagger
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        // API controllers
        app.UseRouting();
        app.UseEndpoints(endpoints => {
            endpoints.MapBlazorHub();
            endpoints.MapFusionWebSocketServer();
            endpoints.MapControllers();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
