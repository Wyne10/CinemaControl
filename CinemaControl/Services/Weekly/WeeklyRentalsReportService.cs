using System.IO;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public class WeeklyRentalsReportService : WeeklyReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTotalToday";
    private const string DateInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";
    private const string ShowRentalsSelector = "select#ReportViewer1_ctl04_ctl07_ddValue";

    public override async Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        
        var sessionPath = GetSessionPath(startDate, endDate);
        var page = await browser.NewPageAsync(); 

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            await page.GotoAsync(ReportUrl);
            var frame = await GetFrame(page);

            var dateString = date.ToString("dd.MM.yyyy 0:00:00");
            await frame.FillAsync(DateInputSelector, dateString);
            await frame.SelectOptionAsync(ShowRentalsSelector, new[] { "1" });

            var newFileName = $"сводный кассовый {date:yyyy-MM-dd}.pdf";
            var newFilePath = Path.Combine(sessionPath, newFileName);
            await SaveReport(page, frame, newFilePath);
        }

        return sessionPath;
    }
}