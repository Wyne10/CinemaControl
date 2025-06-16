using System.IO;
using System.Windows.Controls;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public abstract class WeeklyReportService(ProgressBar progressBar) : IWeeklyReportService
{
    private const string ReportsRootPath = "CinemaControlReports";
        
    private const string ViewReportButtonSelector = "input[name=\"ReportViewer1$ctl04$ctl00\"]";
    private const string ExportMenuLinkSelector = "a#ReportViewer1_ctl05_ctl04_ctl00_ButtonLink";
    private const string PdfLinkSelector = "a[title=\"PDF\"]";

    protected ProgressBar ProgressBar { get; private set; } = progressBar;

    protected string GetSessionPath(DateTime startDate, DateTime endDate)
    {
        var reportsRootPath = Path.Combine(Path.GetTempPath(), ReportsRootPath);
        var sessionPath = Path.Combine(reportsRootPath, $"Отчет_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}");
        Directory.CreateDirectory(sessionPath); 
        return sessionPath;
    }

    protected async Task<IFrame> GetFrame(IPage page)
    {
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

    protected async Task SaveReport(IPage page, IFrame frame, string path)
    {
        await frame.ClickAsync(ViewReportButtonSelector);
        await frame.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await frame.ClickAsync(ExportMenuLinkSelector);
        await frame.Locator(PdfLinkSelector).WaitForAsync();
            
        var downloadTask = page.WaitForDownloadAsync();
        await frame.ClickAsync(PdfLinkSelector);

        var download = await downloadTask;
        await download.SaveAsAsync(path);
    }

    public abstract int GetFilesCount(DateTime startDate, DateTime endDate);

    public abstract Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate, IPage page);
}