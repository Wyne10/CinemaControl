using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using CinemaControl.Configuration;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using CinemaControl.Services.Monthly;
using CinemaControl.Services.Quarterly;
using CinemaControl.Services.Weekly;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaControl;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class MainView
{
    private AppConfigurationWindowBuilder? _configurationWindow;
    public ObservableCollection<TabItem> Tabs { get; } = [];

    public MainView(IOptions<AppConfiguration> appConfiguration, IMovieProvider movieProvider, ILogger<ReportView> logger)
    {
        InitializeComponent();
        DataContext = this;
        _configurationWindow = new AppConfigurationWindowBuilder(appConfiguration);
        AddTab("Еженедельный отчет", new ReportView(new CompositeReportService([new WeeklyRentalsReportService(), new WeeklyCashierReportService(), new WeeklyCardReportService()]), logger));
        AddTab("Ежемесячный отчет", new ReportView(new CompositeReportService([new MonthlyReportService(appConfiguration, movieProvider), new MonthlyPaymentReportService()]), logger));
        AddTab("Ежеквартальный отчет", new ReportView(new QuarterlyReportService(appConfiguration), logger));
    }

    private void AddTab(string header, UserControl reportView)
    {
        var tabItem = new TabItem
        {
            Header = header,
            Content = reportView
        };
        Tabs.Add(tabItem);
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        new ConfigurationWindow(_configurationWindow!).ShowDialog();
    }
}