namespace CinemaControl.Configuration;

public record QuarterlyReportConfiguration
{
    public required string TemplatePath { get; init; }
}