using System.IO;
using CinemaControl.Dtos;
using CinemaControl.Providers.Movie;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;
using ClosedXML.Excel;

namespace CinemaControl.Services.Monthly;

public class MonthlyReportService(IMovieProvider movieProvider) : ReportService, IMonthlyReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FGrossMovieByPeriodn";

    public async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"по сборам за период {from:yyyy-MM-dd} - {to:yyyy-MM-dd}.xlsx";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Excel);
        await download.SaveAsAsync(newFilePath);

        return sessionPath; 
    }

    private IEnumerable<GrossMovieData> ParseGrossMovieData(string grossReportFilePath)
    {
        var grossMovieData = new List<GrossMovieData>();
        using var workbook = new XLWorkbook(grossReportFilePath);
        var worksheet = workbook.Worksheets.First();

        string currentMovieTitle = string.Empty;

        // Пропускаем первые 4 строки (заголовки)
        foreach (var row in worksheet.RowsUsed().Skip(4))
        {
            var firstCell = row.Cell(1);
            var firstCellValue = firstCell.GetValue<string>().Trim();

            // Если ячейка во втором столбце пустая, а в первом нет, и это не "Итог" - это название фильма
            if (!string.IsNullOrWhiteSpace(firstCellValue) && row.Cell(2).IsEmpty())
            {
                if (firstCellValue.ToLower() != "итог")
                {
                    currentMovieTitle = firstCellValue;
                }
            }
            
            // Если в первой ячейке "Итог", это строка с итоговыми данными для фильма
            if (firstCellValue.ToLower() == "итог")
            {
                if (!string.IsNullOrEmpty(currentMovieTitle))
                {
                    var movieData = new GrossMovieData
                    (
                        currentMovieTitle,
                        row.Cell(2).GetValue<int>(),
                        row.Cell(3).GetValue<int>(),
                        row.Cell(4).GetValue<int>()
                    );
                    grossMovieData.Add(movieData);
                    currentMovieTitle = string.Empty; // Сбрасываем для следующего фильма
                }
            }
        }

        return grossMovieData;
    }

}