using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WinampToSpotifyWeb.Models;
using WinampToSpotifyWeb.Services;
using OpenTelemetry;
using OpenTelemetryLib.Metrics;
using OpenTelemetryLib;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.FullName
        });

        builder.Host.UseSerilog((hostContext, services, configuration) =>
        {
            configuration.WriteTo.File("./logs.txt");
        });

        builder.Logging.ClearProviders();

        builder.Services.Configure<SpotifyApiDetails>(builder.Configuration.GetSection("SpotifyApiDetails"));
        builder.Services.TryAddSingleton<ISpotifyService, SpotifyService>();

        // Create logger before building host
        using var loggerFactory = LoggerFactory.Create(config =>
        {
            config.AddConsole(); // Add file, debug, or other providers
            config.AddConfiguration(builder.Configuration.GetSection("Logging"));
        });
        var logger = loggerFactory.CreateLogger<Program>();

        builder.Services.AddOpenTelemetry(logger);
        builder.Services.AddSingleton<IWinampToSpotifyWebMetrics, SpotifyServiceMetrics>(sp =>
                    new SpotifyServiceMetrics(sp.GetRequiredService<ISpotifyService>()));

        builder.Services.AddMvc();

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.AddServiceDefaults();

        var app = builder.Build();

        app.UseCookiePolicy(
                    new CookiePolicyOptions
                    {
                        Secure = CookieSecurePolicy.Always
                    });

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapDefaultEndpoints();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Spotify}/{action=SelectFolder}");

        app.MapRazorPages();

        app.Run();
    }
}