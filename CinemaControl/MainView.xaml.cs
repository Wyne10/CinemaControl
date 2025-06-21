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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaControl;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class MainView
{
    private readonly IConfigurationRoot _configurationRoot;
    public ObservableCollection<TabItem> Tabs { get; } = [];

    public MainView(IConfigurationRoot configurationRoot, IOptions<AppConfiguration> appConfiguration, IMovieProvider movieProvider, ILogger<ReportView> logger)
    {
        InitializeComponent();
        DataContext = this;
        _configurationRoot = configurationRoot;
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
        var appConfiguration = _configurationRoot.GetRequiredSection("App").Get<AppConfiguration>();
        new ConfigurationWindow(new AppConfigurationWindowBuilder(appConfiguration)).ShowDialog();
    }
}