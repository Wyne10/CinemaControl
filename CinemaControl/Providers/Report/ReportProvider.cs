using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public class ReportProvider : IReportProvider
{
    public virtual async Task<IDownload> DownloadReport(IPage page, IFrameLocator frame, ReportSaveType saveType)
    {
        await frame.Locator(IReportProvider.ViewReportButtonSelector).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        const int maxRetries = 5;
        bool exportMenuClicked = false;
        for (int i = 0; i < maxRetries; i++)
        {
            await frame.Locator(IReportProvider.ExportMenuLinkSelector).ClickAsync();
            try
            {
                await frame.Locator(saveType.Selector).WaitForAsync(new() { Timeout = 1000 });
                exportMenuClicked = true;
                break;
            }
            catch (TimeoutException)
            {
                // Ignore timeout and retry
            }
        }

        if (!exportMenuClicked)
        {
            throw new Exception("Не удалось открыть меню экспорта после нескольких попыток");
        }

        var downloadTask = page.WaitForDownloadAsync();
        await frame.Locator(saveType.Selector).ClickAsync();

        return await downloadTask;
    }
}