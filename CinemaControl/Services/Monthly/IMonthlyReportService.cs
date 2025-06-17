using Microsoft.Playwright;

namespace CinemaControl.Services.Monthly;

public interface IMonthlyReportService
{
    Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page);
}