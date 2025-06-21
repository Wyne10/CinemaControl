using System.Windows;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
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
            .ConfigureLogging((_, logging) =>
            {
                logging.AddLog4Net("log4net.config");
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<SettingsService>();
                services.AddSingleton<IMovieProvider, MovieProvider>();
                services.AddSingleton<MainView>();
            })
            .Build();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            var logger = _appHost.Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("Application starting...");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
            throw;
        }


        await _appHost.StartAsync();
        _appHost.Services.GetRequiredService<MainView>().Show();
    }
    
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _appHost.StopAsync();
    }
}