using System.Windows;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CinemaControl;

public partial class App
{
    private readonly IHost _appHost;

    public App()
    {
        _appHost = Host.CreateDefaultBuilder()
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
        await _appHost.StartAsync();
        _appHost.Services.GetRequiredService<MainView>().Show();
    }
    
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _appHost.StopAsync();
    }
}