using System.IO;
using System.Windows.Controls;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyRentalsReportService(ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTotalToday";
    private const string ShowRentalsSelector = "select#ReportViewer1_ctl04_ctl07_ddValue";

    public override int GetFilesCount(DateTime from, DateTime to)
    {
        var count = 0;
        for (var date = from; date <= to; date = date.AddDays(1))
            count++;
        return count;
    }

    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            await page.GotoAsync(ReportUrl);
            var frame = await GetFrame(page);

            await frame.Locator(ShowRentalsSelector).SelectOptionAsync(["1"]);

            var newFileName = $"сводный кассовый {date:dd-MM-yy}.pdf";
            var newFilePath = Path.Combine(sessionPath, newFileName);
            var reportProvider = new SingleReportProvider(date);
            var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
            await download.SaveAsAsync(newFilePath);
            
            ProgressBar.Value++;
        }

        return sessionPath;
    }
}