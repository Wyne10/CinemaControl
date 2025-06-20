﻿using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public class PeriodReportProvider(DateTime from, DateTime to) : ReportProvider
{
    private const string DateFromInputSelector = "input[name=\"ReportViewer1$ctl04$ctl03$txtValue\"]";
    private const string DateToInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";
    
    public override async Task<IDownload> DownloadReport(IPage page, IFrameLocator frame, ReportSaveType saveType)
    {
        var dateToString = to.ToString("dd.MM.yyyy 0:00:00");
        await frame.Locator(DateToInputSelector).FillAsync(dateToString);
        var dateFromString = from.ToString("dd.MM.yyyy 0:00:00");
        await frame.Locator(DateFromInputSelector).FillAsync(dateFromString);
        // Double-clicking view report button due to poor site implementation
        await frame.Locator(IReportProvider.ViewReportButtonSelector).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        return await base.DownloadReport(page, frame, saveType);
    }
}