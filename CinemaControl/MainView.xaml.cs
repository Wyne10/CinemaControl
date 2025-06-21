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

namespace CinemaControl;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class MainView
{
    private readonly IConfiguration _configuration;
    public ObservableCollection<TabItem> Tabs { get; } = [];

    public MainView(IConfiguration configuration, IMovieProvider movieProvider, ILogger<ReportView> logger)
    {
        InitializeComponent();
        DataContext = this;
        _configuration = configuration;
        try
        {
            var monthlyReportConfiguration =
                _configuration.GetRequiredSection("MonthlyReport").Get<MonthlyReportConfiguration>();
            var quarterlyReportConfiguration =
                _configuration.GetRequiredSection("QuarterlyReport").Get<QuarterlyReportConfiguration>();
            AddTab("Еженедельный отчет",
                new ReportView(
                    new CompositeReportService([
                        new WeeklyRentalsReportService(), new WeeklyCashierReportService(),
                        new WeeklyCardReportService()
                    ]), new WeeklyReportConfigurationWindowBuilder(), logger));
            AddTab("Ежемесячный отчет",
                new ReportView(
                    new CompositeReportService([
                        new MonthlyReportService(monthlyReportConfiguration!, movieProvider),
                        new MonthlyPaymentReportService()
                    ]), new MonthlyReportConfigurationWindowBuilder(monthlyReportConfiguration!), logger));
            AddTab("Ежеквартальный отчет",
                new ReportView(new QuarterlyReportService(quarterlyReportConfiguration!),
                    new QuarterlyReportConfigurationWindowBuilder(quarterlyReportConfiguration!), logger));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to build main report views");
        }
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
        var appConfiguration = _configuration.GetRequiredSection("App").Get<AppConfiguration>();
        new ConfigurationWindow(new AppConfigurationWindowBuilder(appConfiguration!)).ShowDialog();
    }
}