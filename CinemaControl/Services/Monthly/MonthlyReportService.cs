using System.Collections.Immutable;
using System.IO;
using CinemaControl.Dtos;
using CinemaControl.Providers.Movie;
using CinemaControl.Providers.Report;
using Microsoft.Playwright;
using ClosedXML.Excel;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace CinemaControl.Services.Monthly;

public class MonthlyReportService(SettingsService settingsService, IMovieProvider movieProvider) : ReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FGrossMovieByPeriod";
    private const string CardReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FMovieByPeriodPushkin";

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

        var grossMovieData = ParseGrossMovieData(newFilePath);
        await FillMonthlyReport(grossMovieData, from, to, page);
        
        ProgressDownload();

        return sessionPath;
    }

    private async Task<string> FillMonthlyReport(IReadOnlyCollection<GrossMovieData> grossMovieData, DateTime from, DateTime to, IPage page)
    {
        var templatePath = settingsService.Settings.MonthlyReportTemplatePath;
        if (string.IsNullOrWhiteSpace(templatePath))
        {
            throw new Exception("Не установлен путь к шаблону ежемесячного отчета.");
        }

        using var document = DocX.Load(templatePath);

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

        document.ReplaceText(new StringReplaceTextOptions
            { SearchValue = "{{month}}", NewValue = System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(from.Month) });
        document.ReplaceText(new StringReplaceTextOptions
            { SearchValue = "{{year}}", NewValue = from.Year.ToString() }); 
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_total}}", NewValue = sessionTotal.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_total}}", NewValue = viewerTotal.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{movie_russian}}", NewValue = russianMovies.Count.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_card}}", NewValue = (await GetCardViewerCount(from, to, page)).ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_russian}}", NewValue = russianMovies.Sum(data => data.SessionCount).ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_russian}}", NewValue = russianMovies.Sum(data => data.ViewerCount).ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{movie_foreign}}", NewValue = foreignMovies.Count.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_foreign}}", NewValue = foreignMovies.Sum(data => data.SessionCount).ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_foreign}}", NewValue = foreignMovies.Sum(data => data.ViewerCount).ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_children}}", NewValue = sessionChildren.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_children}}", NewValue = viewerChildren.ToString() }); 
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_teenagers}}", NewValue = sessionTeenagers.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_teenagers}}", NewValue = viewerTeenagers.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{session_adults}}", NewValue = sessionAdults.ToString() });
        document.ReplaceText(new StringReplaceTextOptions 
            { SearchValue = "{{viewer_adults}}", NewValue = viewerAdults.ToString() }); 
        
        var newFileName = $"Таблица отчетности в УК {System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(from.Month)} {from:yyyy}г.docx";
        var newFilePath = Path.Combine(GetSessionPath(from, to), newFileName);
        document.SaveAs(newFilePath);

        return newFilePath;
    }

    public static HashSet<GrossMovieData> ParseGrossMovieData(string grossReportFilePath)
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
                        currentMovieTitle[..^2].Trim(),
                        currentMovieTitle.Substring(currentMovieTitle.Length - 2, 2).Trim(),
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

    private async Task<int> GetCardViewerCount(DateTime from, DateTime to, IPage page)
    {
        var sessionPath = GetSessionPath(from, to);
        
        await page.GotoAsync(CardReportUrl);
        var frame = await GetFrame(page);

        var newFileName = $"По пушкинской {from:dd.MM.yy} - {to:dd.MM.yy}.xlsx";
        var newFilePath = Path.Combine(sessionPath, newFileName);
        var reportProvider = new PeriodReportProvider(from, to);
        var download = await reportProvider.DownloadReport(page, frame, ReportSaveType.Excel);
        await download.SaveAsAsync(newFilePath);
        
        using var workbook = new XLWorkbook(newFilePath);
        var worksheet = workbook.Worksheets.First();
        
        var lastRow = worksheet.LastRowUsed();
        if (lastRow == null || lastRow.Cell(4).IsEmpty())
            return 0;
        return lastRow.Cell(4).GetValue<int>();
    }
}