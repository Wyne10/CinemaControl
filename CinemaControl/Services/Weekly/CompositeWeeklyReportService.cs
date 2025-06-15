using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class CompositeWeeklyReportService(IEnumerable<IWeeklyReportService> weeklyReportServices) : WeeklyReportService
{
    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate, IPage page)
    {
        foreach(IWeeklyReportService weeklyReportService in weeklyReportServices) await weeklyReportService.GetReportFilesAsync(startDate, endDate, page);
        return GetSessionPath(startDate, endDate);
    }
}