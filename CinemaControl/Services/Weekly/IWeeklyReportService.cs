using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public interface IWeeklyReportService
{
    int GetFilesCount(DateTime startDate, DateTime endDate);
    Task<string> GetReportFiles(DateTime startDate, DateTime endDate, IPage page);
}