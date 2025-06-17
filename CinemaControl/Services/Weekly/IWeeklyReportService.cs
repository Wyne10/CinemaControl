using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public interface IWeeklyReportService
{
    int GetFilesCount(DateTime from, DateTime to);
    Task<string> GetReportFiles(DateTime from, DateTime to, IPage page);
}