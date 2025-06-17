using Microsoft.Playwright;

namespace CinemaControl.Services;

public interface IReportService
{
    Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page);
}