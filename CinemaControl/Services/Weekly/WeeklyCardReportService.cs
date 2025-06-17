using System.IO;
using System.Windows.Controls;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCardReportService(ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FMovieByPeriodPushkin";

    public override int GetFilesCount(DateTime from, DateTime to) => 1;

    public override async Task<string> GetReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"по пушкинской {from:yyyy-MM-dd} - {to:yyyy-MM-dd}.pdf";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Pdf);
        await download.SaveAsAsync(newFilePath);
        
        ProgressBar.Value++;

        return sessionPath;
    }
}