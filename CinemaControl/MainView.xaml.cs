using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using CinemaControl.Services.Monthly;
using CinemaControl.Services.Quarterly;
using CinemaControl.Services.Weekly;

namespace CinemaControl;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class MainView
{
    public ObservableCollection<TabItem> Tabs { get; } = [];

    public MainView(SettingsService settingsService, IMovieProvider movieProvider)
    {
        InitializeComponent();
        AddTab("Еженедельный отчет", new ReportView(new CompositeReportService([new WeeklyRentalsReportService(), new WeeklyCashierReportService(), new WeeklyCardReportService()])));
        AddTab("Ежемесячный отчет", new ReportView(new CompositeReportService([new MonthlyReportService(settingsService, movieProvider), new MonthlyPaymentReportService()])));
        AddTab("Ежеквартальный отчет", new ReportView(new QuarterlyReportService(settingsService)));
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
}