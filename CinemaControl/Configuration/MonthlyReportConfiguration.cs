namespace CinemaControl.Configuration;

public record MonthlyReportConfiguration
{
    public required string TemplatePath { get; init; }
}