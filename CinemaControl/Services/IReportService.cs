using Microsoft.Playwright;

namespace CinemaControl.Services;

public interface IReportService
{
    string GetSessionPath(DateTime from, DateTime to);
    Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page);
}