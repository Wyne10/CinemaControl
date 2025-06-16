using System.Windows;
using CinemaControl.Services;
using Microsoft.Win32;

namespace CinemaControl
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            
            // Load current settings into UI
            ApiTokenTextBox.Text = _settingsService.Settings.ApiToken;
            TemplatePathTextBox.Text = _settingsService.Settings.MonthlyReportTemplatePath;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Settings.ApiToken = ApiTokenTextBox.Text;
            _settingsService.Settings.MonthlyReportTemplatePath = TemplatePathTextBox.Text;
            _settingsService.SaveSettings();
            DialogResult = true;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx|All files (*.*)|*.*",
                Title = "Выберите шаблон ежемесячного отчета"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TemplatePathTextBox.Text = openFileDialog.FileName;
            }
        }
    }
} 