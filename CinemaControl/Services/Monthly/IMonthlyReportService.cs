using Microsoft.Playwright;

namespace CinemaControl.Services.Monthly;

public interface IMonthlyReportService
{
    Task<string> GenerateReportFile(DateTime date, IPage page);
}