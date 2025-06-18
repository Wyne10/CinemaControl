using System.Windows;
using System.Windows.Controls;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using CinemaControl.Services.Monthly;

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
            var movieProvider = new MovieProvider(_settingsService);
            AddTab("Еженедельный отчет", new WeeklyReportView());
            AddTab("Ежемесячный отчет", new ReportView(new MonthlyReportService(_settingsService, movieProvider)));
        }

        private void AddTab(string header, UserControl reportView)
        {
            var tabItem = new TabItem
            {
                Header = header,
                Content = reportView
            };
            MainTabControl.Items.Add(tabItem);
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