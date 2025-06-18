using System.IO;
using Microsoft.Playwright;

namespace CinemaControl.Services;

public abstract class ReportService : IReportService
{
    private const string ReportsRootPath = "CinemaControlReports";
    
    private const string UserNameSelector = "input[name=\"UserName\"]";
    private const string LogInSelector = "input[type=\"submit\"]";
    
    protected string GetSessionPath(DateTime startDate, DateTime endDate)
    {
        var reportsRootPath = Path.Combine(Path.GetTempPath(), ReportsRootPath);
        var sessionPath = Path.Combine(reportsRootPath, $"Отчет_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}");
        Directory.CreateDirectory(sessionPath); 
        return sessionPath;
    }

    protected async Task<IFrameLocator> GetFrame(IPage page)
    {
        if (await page.Locator(UserNameSelector).IsVisibleAsync())
        {
            await page.Locator(UserNameSelector).FillAsync("Администратор");
            await page.Locator(LogInSelector).ClickAsync();
        }
        
        var frameLocator = page.FrameLocator("iframe");

        return frameLocator;
    }

    public abstract Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page);
}