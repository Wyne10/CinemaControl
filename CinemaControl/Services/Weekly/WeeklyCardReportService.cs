using System.IO;
using System.Windows.Controls;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCardReportService(ProgressBar progressBar) : WeeklyReportService(progressBar)
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FMovieByPeriodPushkin";
    private const string DateFromInputSelector = "input[name=\"ReportViewer1$ctl04$ctl03$txtValue\"]";
    private const string DateToInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";

    public override int GetFilesCount(DateTime startDate, DateTime endDate) => 1;

    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate, IPage page)
    {
        var sessionPath = GetSessionPath(startDate, endDate);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var dateFromString = startDate.ToString("dd.MM.yyyy 0:00:00");
        await frame.FillAsync(DateFromInputSelector, dateFromString);
        
        var dateToString = endDate.ToString("dd.MM.yyyy 0:00:00");
        await frame.FillAsync(DateToInputSelector, dateToString);

        var newFileName = $"по пушкинской {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}.pdf";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        await SaveReport(page, frame, newFilePath);
        ProgressBar.Value++;

        return sessionPath;
    }
}