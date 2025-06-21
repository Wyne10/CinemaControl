using Microsoft.Playwright;

namespace CinemaControl.Services;

public interface IReportService
{
    event Action OnDownloadProgress;
    string GetSessionPath(DateTime from, DateTime to);
    Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page);
}