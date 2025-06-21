using System.Windows;
using CinemaControl.Configuration;
using CinemaControl.Providers.Movie;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CinemaControl;

public partial class App
{
    private readonly IHost _appHost;

    public App()
    {
        _appHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureLogging((_, logger) =>
            {
                logger.ClearProviders();
                logger.AddLog4Net("log4net.config");
                logger.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppConfiguration>(
                    context.Configuration.GetRequiredSection("App"));
                services.Configure<WeeklyReportConfiguration>(
                    context.Configuration.GetRequiredSection("WeeklyReport"));
                services.Configure<MonthlyReportConfiguration>(
                    context.Configuration.GetRequiredSection("MonthlyReport"));
                services.Configure<QuarterlyReportConfiguration>(
                    context.Configuration.GetRequiredSection("QuarterlyReport"));
                services.AddSingleton<IMovieProvider, MovieProvider>();
                services.AddSingleton<MainView>();
            })
            .Build();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var logger = _appHost.Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("Application starting...");

        await _appHost.StartAsync();
        _appHost.Services.GetRequiredService<MainView>().Show();
    }
    
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _appHost.StopAsync();
    }
}