using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using CinemaControl.ViewModel;

namespace CinemaControl;

public partial class MainView
{
    public MainView(SettingsService settingsService, IMovieProvider movieProvider)
    {
        InitializeComponent();
        DataContext = new MainViewModel(settingsService, movieProvider);
    }

}