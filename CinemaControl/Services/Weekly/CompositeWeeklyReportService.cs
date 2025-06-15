namespace CinemaControl.Services.Weekly;

public class CompositeWeeklyReportService(IEnumerable<IWeeklyReportService> weeklyReportServices) : WeeklyReportService
{
    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate)
    {
        foreach(IWeeklyReportService weeklyReportService in weeklyReportServices) await weeklyReportService.GetReportFilesAsync(startDate, endDate);
        return GetSessionPath(startDate, endDate);
    }
}