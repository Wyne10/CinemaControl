using Microsoft.Playwright;
using System.IO;

namespace CinemaControl.Services
{
    public class WeeklyReportService : IWeeklyReportService
    {
        private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTotalToday";
        private const string DateInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";
        private const string ViewReportButtonSelector = "input[name=\"ReportViewer1$ctl04$ctl00\"]";
        private const string ShowRentalsSelector = "select#ReportViewer1_ctl04_ctl07_ddValue";
        private const string ExportMenuLinkSelector = "a#ReportViewer1_ctl05_ctl04_ctl00_ButtonLink";
        private const string PdfLinkSelector = "a[title=\"PDF\"]";

        public async Task<IEnumerable<string>> GetReportFilesAsync(DateTime startDate, DateTime endDate)
        {
            // Установка Playwright, если это необходимо
            Program.Main(new[] { "install" });

            var filePaths = new List<string>();
            var tempPath = Path.Combine(Path.GetTempPath(), "CinemaControlReports");
            Directory.CreateDirectory(tempPath);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                await page.GotoAsync(ReportUrl);

                // Ожидаем, пока пользователь вручную войдет в систему и на странице появится iframe.
                await page.WaitForSelectorAsync("iframe", new() { Timeout = 30000 });

                // Получаем элемент iframe
                var iframeElement = await page.QuerySelectorAsync("iframe");
                if (iframeElement == null)
                {
                    throw new Exception("Не удалось найти элемент iframe на странице.");
                }

                // Получаем контент фрейма из элемента
                var frame = await iframeElement.ContentFrameAsync();
                if (frame == null)
                {
                    throw new Exception("Не удалось получить контекст iframe. Возможно, он еще не загрузился.");
                }

                // Теперь все действия выполняем в контексте этого фрейма
                var dateString = date.ToString("dd.MM.yyyy 0:00:00");
                await frame.FillAsync(DateInputSelector, dateString);

                // Выбираем "Показать" в выпадающем списке по его ID
                await frame.SelectOptionAsync(ShowRentalsSelector, new[] { "1" });

                // Нажимаем "Просмотр отчета"
                await frame.ClickAsync(ViewReportButtonSelector);
                
                await frame.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Нажимаем на ссылку, которая открывает меню экспорта, используя ее точный ID.
                await frame.ClickAsync(ExportMenuLinkSelector);

                // Ждем, пока появится ссылка для скачивания PDF, и нажимаем на нее.
                await frame.Locator(PdfLinkSelector).WaitForAsync();

                var downloadTask = page.WaitForDownloadAsync();
                await frame.ClickAsync(PdfLinkSelector);

                var download = await downloadTask;
                
                var newFileName = $"report_{date:yyyy-MM-dd}.pdf";
                var newFilePath = Path.Combine(tempPath, newFileName);

                await download.SaveAsAsync(newFilePath);
                filePaths.Add(newFilePath);
            }

            return filePaths;
        }
    }
} 