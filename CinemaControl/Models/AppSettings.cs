namespace CinemaControl.Models;

public class AppSettings
{
    public string? ApiToken { get; set; }
    public string? MonthlyReportTemplatePath { get; set; }
    public string? QuarterlyReportTemplatePath { get; set; }
}