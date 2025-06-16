using System.Windows.Controls;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class CompositeWeeklyReportService(IEnumerable<IWeeklyReportService> weeklyReportServices, ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    public override int GetFilesCount(DateTime startDate, DateTime endDate) =>
        weeklyReportServices
            .Select(w => w.GetFilesCount(startDate, endDate))
            .Aggregate((acc, next) => acc + next);

    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate, IPage page)
    {
        foreach(IWeeklyReportService weeklyReportService in weeklyReportServices) await weeklyReportService.GetReportFilesAsync(startDate, endDate, page);
        return GetSessionPath(startDate, endDate);
    }
}