using System.IO;
using System.Windows.Controls;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCashierReportService(ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTodayByUsers";

    public override int GetFilesCount(DateTime startDate, DateTime endDate)
    {
        var count = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
            count++;
        return count;
    }
    
    public override async Task<string> GetReportFiles(DateTime startDate, DateTime endDate, IPage page)
    {
        var sessionPath = GetSessionPath(startDate, endDate);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            await page.GotoAsync(ReportUrl);
            var frame = await GetFrame(page);

            var newFileName = $"разбивкой по кассирам {date:yyyy-MM-dd}.pdf";
            var newFilePath = Path.Combine(sessionPath, newFileName);
            var reportProvider = new SingleReportProvider(date);
            var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
            await download.SaveAsAsync(newFilePath);
            
            ProgressBar.Value++;
        }

        return sessionPath;
    }
}