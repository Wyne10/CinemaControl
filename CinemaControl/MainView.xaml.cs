using System.Windows;
using CinemaControl.Services;

namespace CinemaControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly SettingsService _settingsService;

        public MainView()
        {
            InitializeComponent();
            _settingsService = new SettingsService();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settingsService)
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }
    }
}