using Microsoft.Extensions.Options;

namespace CinemaControl.Configuration;

public class ConfigurationService(
    IOptionsMonitor<AppConfiguration> appConfiguration,
    IOptionsMonitor<MonthlyReportConfiguration> monthlyReportConfiguration,
    IOptionsMonitor<QuarterlyReportConfiguration> quarterlyReportConfiguration)
{
    public AppConfiguration AppConfiguration => appConfiguration.CurrentValue;
    public MonthlyReportConfiguration MonthlyReportConfiguration => monthlyReportConfiguration.CurrentValue;
    public QuarterlyReportConfiguration QuarterlyReportConfiguration => quarterlyReportConfiguration.CurrentValue;
}