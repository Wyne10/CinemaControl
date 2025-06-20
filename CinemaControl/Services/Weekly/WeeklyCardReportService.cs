using System.IO;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCardReportService : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FMovieByPeriodPushkin";

    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"По пушкинской {from:dd.MM.yy} - {to:dd.MM.yy}.pdf";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
        await download.SaveAsAsync(newFilePath);
        
        ProgressDownload();

        return sessionPath;
    }
}