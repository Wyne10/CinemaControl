using System.Windows;
using CinemaControl.Services;

namespace CinemaControl
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            ApiTokenTextBox.Text = _settingsService.Settings.ApiToken;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Settings.ApiToken = ApiTokenTextBox.Text;
            _settingsService.SaveSettings();
            DialogResult = true;
        }
    }
} 