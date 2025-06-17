using System.Windows.Controls;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class CompositeWeeklyReportService(IEnumerable<IWeeklyReportService> weeklyReportServices, ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    public override int GetFilesCount(DateTime from, DateTime to) =>
        weeklyReportServices
            .Select(w => w.GetFilesCount(from, to))
            .Aggregate((acc, next) => acc + next);

    public override async Task<string> GetReportFiles(DateTime from, DateTime to, IPage page)
    {
        foreach(IWeeklyReportService weeklyReportService in weeklyReportServices) await weeklyReportService.GetReportFiles(from, to, page);
        return GetSessionPath(from, to);
    }
}