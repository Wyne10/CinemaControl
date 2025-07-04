using System.IO;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Reports.Monthly;

public class MonthlyPaymentReportService : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FPaymentTypesByPeriod";

    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"По видам оплат {System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(from.Month)} {from.Year}.pdf";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
        await download.SaveAsAsync(newFilePath);
        
        ProgressDownload();
        
        return sessionPath;
    }
}