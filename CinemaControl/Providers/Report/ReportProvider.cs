using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public class ReportProvider : IReportProvider
{
    public virtual async Task<IDownload> DownloadReport(IPage page, IFrame frame, ReportSaveType saveType)
    {
        await frame.ClickAsync(IReportProvider.ViewReportButtonSelector);
        await frame.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await frame.ClickAsync(IReportProvider.ExportMenuLinkSelector);
        await frame.WaitForSelectorAsync(saveType.Selector);
            
        var downloadTask = page.WaitForDownloadAsync();
        await frame.ClickAsync(saveType.Selector);

        return await downloadTask;
    }
}