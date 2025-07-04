using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CinemaControl.Configuration;
using CinemaControl.Providers.Movie;
using CinemaControl.Reports;
using CinemaControl.Reports.Monthly;
using CinemaControl.Reports.Quarterly;
using CinemaControl.Reports.Weekly;
using Microsoft.Extensions.Logging;

namespace CinemaControl;

public partial class MainView
{
    private readonly ConfigurationService _configuration;
    
    public ObservableCollection<TabItem> Tabs { get; } = [];

    public MainView(ConfigurationService configuration, IMovieProvider movieProvider, ILogger<ReportView> logger)
    {
        InitializeComponent();
        DataContext = this;
        _configuration = configuration;
        try
        {
            AddTab("Еженедельный отчет",
                new ReportView(
                    new CompositeReportService([
                        new WeeklyRentalsReportService(), new WeeklyCashierReportService(), new WeeklyCardReportService()
                    ]), new WeeklyReportConfigurationWindowBuilder(), configuration, logger));
            AddTab("Ежемесячный отчет",
                new ReportView(
                    new CompositeReportService([
                        new MonthlyReportService(configuration, movieProvider), new MonthlyPaymentReportService()
                    ]), new MonthlyReportConfigurationWindowBuilder(configuration), configuration, logger));
            AddTab("Ежеквартальный отчет",
                new ReportView(
                    new QuarterlyReportService(configuration),
                    new QuarterlyReportConfigurationWindowBuilder(configuration), configuration, logger));
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
        new ConfigurationWindow(new AppConfigurationWindowBuilder(_configuration)).ShowDialog();
    }
}