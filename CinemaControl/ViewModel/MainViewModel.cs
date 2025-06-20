using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using CinemaControl.Providers.Movie;
using CinemaControl.Services;
using CinemaControl.Services.Monthly;
using CinemaControl.Services.Quarterly;

namespace CinemaControl.ViewModel;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class MainViewModel
{
    public ObservableCollection<TabItem> Tabs { get; } = [];
    
    public MainViewModel(SettingsService settingsService, IMovieProvider movieProvider)
    {
        AddTab("Еженедельный отчет", new WeeklyReportView());
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