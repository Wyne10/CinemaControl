using System.IO;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyCashierReportService : WeeklyReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTodayByUsers";
    private const string DateInputSelector = "input[name=\"ReportViewer1$ctl04$ctl03$txtValue\"]";

    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate, IPage page)
    {
        var sessionPath = GetSessionPath(startDate, endDate);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            await page.GotoAsync(ReportUrl);
            var frame = await GetFrame(page);

            var dateString = date.ToString("dd.MM.yyyy 0:00:00");
            await frame.FillAsync(DateInputSelector, dateString);

            var newFileName = $"разбивкой по кассирам {date:yyyy-MM-dd}.pdf";
            var newFilePath = Path.Combine(sessionPath, newFileName);
            await SaveReport(page, frame, newFilePath);
        }

        return sessionPath;
    }
}