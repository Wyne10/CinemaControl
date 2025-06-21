using System.IO;
using CinemaControl.Dtos;
using CinemaControl.Providers.Report;
using CinemaControl.Services.Monthly;
using ClosedXML.Excel;
using Microsoft.Playwright;

namespace CinemaControl.Services.Quarterly;

public class QuarterlyReportService(SettingsService settingsService) : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FGrossMovieByPeriod";
    
    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"По сборам за период {from:dd.MM.yy} - {to:dd.MM.yy}.xlsx";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Excel);
        await download.SaveAsAsync(newFilePath);

        ProgressDownload();
        
        var grossMovieData = MonthlyReportService.ParseGrossMovieData(newFilePath);
        FillQuarterlyReport(grossMovieData, from, to);

        ProgressDownload();
        
        return sessionPath;
    } 
    
    private string FillQuarterlyReport(IReadOnlyCollection<GrossMovieData> grossMovieData, DateTime from, DateTime to)
    {
        var templatePath = settingsService.Settings.QuarterlyReportTemplatePath;
        if (string.IsNullOrWhiteSpace(templatePath))
        {
            throw new Exception("Не установлен путь к шаблону ежеквартального отчета.");
        }

        using var workbook = new XLWorkbook(templatePath);
        var worksheet = workbook.Worksheets.First();

        worksheet.Row(12).Cell("C").Value = from.ToString("dd.MM.yy");
        worksheet.Row(12).Cell("E").Value = to.ToString("dd.MM.yy");
        
        var currentRowNumber = 15;
        
        foreach (var movieData in grossMovieData)
        {
            var row = worksheet.Row(currentRowNumber);
            row.InsertRowsBelow(1);
            row.CopyTo(worksheet.Row(currentRowNumber + 1));

            row.Cell("A").Value = currentRowNumber - 14;
            row.Cell("B").Value = $"{movieData.MovieName} {movieData.ScreenType}";
            row.Cell("G").Value = movieData.SessionCount;
            row.Cell("H").Value = movieData.ViewerCount;
            row.Cell("I").Value = movieData.Gross;
            
            var rateCell = row.Cell("J");
            rateCell.Value = .0075;
            rateCell.Style.NumberFormat.Format = "0.00%";

            var remunerationCell = row.Cell("K");
            remunerationCell.FormulaA1 = $"I{currentRowNumber}*J{currentRowNumber}";
            worksheet.Row(currentRowNumber + 2).Cell("I").FormulaA1 = $"SUM(I15:I{currentRowNumber})";
            worksheet.Row(currentRowNumber + 2).Cell("K").FormulaA1 = $"SUM(K15:K{currentRowNumber})";
            currentRowNumber++;
        }
        
        var newFileName = $"Отчет об использовании аудиовизуальных произведений {from:dd.MM.yy} - {to:dd.MM.yy}.xlsx";
        var newFilePath = Path.Combine(GetSessionPath(from, to), newFileName);
        workbook.SaveAs(newFilePath);

        return newFilePath;
    }
}