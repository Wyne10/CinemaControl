using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public class ReportProvider : IReportProvider
{
    public virtual async Task<IDownload> DownloadReport(IPage page, IFrame frame, ReportSaveType saveType)
    {
        await frame.Locator(IReportProvider.ViewReportButtonSelector).ClickAsync();
        await frame.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await frame.Locator(IReportProvider.ExportMenuLinkSelector).ClickAsync();
        await frame.Locator(saveType.Selector).WaitForAsync();
            
        var downloadTask = page.WaitForDownloadAsync();
        await frame.Locator(saveType.Selector).ClickAsync();

        return await downloadTask;
    }
}