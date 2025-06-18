using System.Collections.Immutable;
using System.IO;
using CinemaControl.Dtos;
using CinemaControl.Providers.Movie;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;
using ClosedXML.Excel;
using SautinSoft.Document;
using System.Text.RegularExpressions;

namespace CinemaControl.Services.Monthly;

public class MonthlyReportService(SettingsService settingsService, IMovieProvider movieProvider) : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FGrossMovieByPeriod";

    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);

        await page.GotoAsync(ReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"по сборам за период {from:yyyy-MM-dd} - {to:yyyy-MM-dd}.xlsx";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Excel);
        await download.SaveAsAsync(newFilePath);

        var grossMovieData = ParseGrossMovieData(newFilePath);
        await FillMonthlyReport(grossMovieData, from, to);

        return sessionPath;
    }

    private async Task<string> FillMonthlyReport(IReadOnlyCollection<GrossMovieData> grossMovieData, DateTime from, DateTime to)
    {
        var templatePath = settingsService.Settings.MonthlyReportTemplatePath;
        if (string.IsNullOrWhiteSpace(templatePath))
        {
            throw new Exception("Не установлен путь к шаблону ежемесячного отчета.");
        }

        var document = DocumentCore.Load(templatePath);

        var movies = await movieProvider.GetMovies(grossMovieData.Select(data => data.MovieName));
        var russianMovies = grossMovieData.Where(data => movies[data.MovieName].IsRussian()).ToImmutableHashSet();
        var foreignMovies = grossMovieData.Where(data => !movies[data.MovieName].IsRussian()).ToImmutableHashSet();

        var sessionTotal = grossMovieData.Sum(data => data.SessionCount);
        var viewerTotal = grossMovieData.Sum(data => data.ViewerCount);
        var sessionChildren = grossMovieData
            .Where(data => movies[data.MovieName].IsChildrenAvailable())
            .Sum(data => data.SessionCount);
        var viewerChildren = grossMovieData
            .Where(data => movies[data.MovieName].IsChildrenAvailable())
            .Sum(data => data.ViewerCount);
        var sessionTeenagers = (int)((sessionTotal - sessionChildren) * 0.7);
        var viewerTeenagers = (int)((viewerTotal - viewerChildren) * 0.7);
        var sessionAdults = sessionTotal - sessionChildren - sessionTeenagers;
        var viewerAdults = viewerTotal - viewerChildren - viewerTeenagers;

        var replacements = new Dictionary<string, string>
        {
            { "{{month}}", System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(from.Month) },
            { "{{year}}", from.Year.ToString() },
            { "{{session_total}}", sessionTotal.ToString() },
            { "{{viewer_total}}", viewerTotal.ToString() },
            { "{{movie_russian}}", russianMovies.Count.ToString() },
            { "{{session_russian}}", russianMovies.Sum(data => data.SessionCount).ToString() },
            { "{{viewer_russian}}", russianMovies.Sum(data => data.ViewerCount).ToString() },
            { "{{movie_foreign}}", foreignMovies.Count.ToString() },
            { "{{session_foreign}}", foreignMovies.Sum(data => data.SessionCount).ToString() },
            { "{{viewer_foreign}}", foreignMovies.Sum(data => data.ViewerCount).ToString() },
            { "{{session_children}}", sessionChildren.ToString() },
            { "{{viewer_children}}", viewerChildren.ToString() },
            { "{{session_teenagers}}", sessionTeenagers.ToString() },
            { "{{viewer_teenagers}}", viewerTeenagers.ToString() },
            { "{{session_adults}}", sessionAdults.ToString() },
            { "{{viewer_adults}}", viewerAdults.ToString() }
        };

        foreach (var (placeholder, value) in replacements)
        {
            Regex regex = new Regex(Regex.Escape(placeholder), RegexOptions.IgnoreCase);
            foreach (var range in document.Content.Find(regex).Reverse())
            {
                range.Replace(value);
            }
        }
        
        var newFileName = $"Таблица отчетности в УК {System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(from.Month)} {from:yyyy}г.docx";
        var newFilePath = Path.Combine(GetSessionPath(from, to), newFileName);
        document.Save(newFilePath);

        return newFilePath;
    }

    private static HashSet<GrossMovieData> ParseGrossMovieData(string grossReportFilePath)
    {
        var grossMovieData = new HashSet<GrossMovieData>();
        using var workbook = new XLWorkbook(grossReportFilePath);
        var worksheet = workbook.Worksheets.First();

        string currentMovieTitle = string.Empty;

        // TODO Use RowsUsed instead?
        // Пропускаем первые 4 строки (заголовки)
        foreach (var row in worksheet.Rows().Skip(4))
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