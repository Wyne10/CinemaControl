using System.IO;
using Microsoft.Playwright;

namespace CinemaControl.Services;

public abstract class ReportService
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

    protected async Task<IFrame> GetFrame(IPage page)
    {
        if (await page.Locator(UserNameSelector).IsVisibleAsync())
        {
            await page.FillAsync(UserNameSelector, "Администратор");
            await page.ClickAsync(LogInSelector);
        }
        
        var iframeElement = await page.WaitForSelectorAsync("iframe", new() { Timeout = 30000 });
        if (iframeElement == null)
        {
            throw new Exception("Превышено время ожидания.");
        }

        var frame = await iframeElement.ContentFrameAsync();
        if (frame == null)
        {
            throw new Exception("Не удалось получить контекст iframe. Возможно, он еще не загрузился.");
        }

        return frame;
    }
}