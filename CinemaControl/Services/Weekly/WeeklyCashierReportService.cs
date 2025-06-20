using System.IO;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCashierReportService : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTodayByUsers";

    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);
        Directory.CreateDirectory(sessionPath);

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            await page.GotoAsync(ReportUrl);
            var frame = await GetFrame(page);

            var newFileName = $"Разбивкой по кассирам {date:dd.MM.yy}.pdf";
            var newFilePath = Path.Combine(sessionPath, newFileName);
            var reportProvider = new SingleReportProvider(date, "input[name=\"ReportViewer1$ctl04$ctl03$txtValue\"]");
            var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
            await download.SaveAsAsync(newFilePath);
            
            ProgressDownload();
        }

        return sessionPath;
    }
}