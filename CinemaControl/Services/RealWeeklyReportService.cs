using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CinemaControl.Services
{
    public class RealWeeklyReportService : IWeeklyReportService
    {
        private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=CashReports%2FCashTotalToday";
        private const string DateInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";
        private const string ViewReportButtonSelector = "input[name=\"ReportViewer1$ctl04$ctl00\"]"; // Предположение, может потребовать уточнения

        public async Task<IEnumerable<string>> GetReportFilesAsync(DateTime startDate, DateTime endDate)
        {
            // Установка Playwright, если это необходимо
            Program.Main(new[] { "install" });

            var filePaths = new List<string>();
            var tempPath = Path.Combine(Path.GetTempPath(), "CinemaControlReports");
            Directory.CreateDirectory(tempPath);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false }); // Headless = false для наглядности
            var page = await browser.NewPageAsync();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                await page.GotoAsync(ReportUrl);

                // Ожидаем, пока пользователь вручную войдет в систему и появится поле для ввода даты.
                // Увеличиваем таймаут, чтобы у пользователя было время на ввод данных.
                var dateInput = page.Locator(DateInputSelector);
                await dateInput.WaitForAsync(new() { Timeout = 30000 }); // 30 секунд

                // Ввод даты
                var dateString = date.ToString("dd.MM.yyyy 0:00:00");
                await dateInput.FillAsync(dateString);

                // Нажатие кнопки "Просмотр отчета"
                await page.ClickAsync(ViewReportButtonSelector);
                
                // Ожидаем, пока отчет обновится. Можно использовать page.WaitForLoadStateAsync() или ожидать конкретный элемент
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Ожидаем скачивание и выполняем скрипт экспорта
                var downloadTask = page.WaitForDownloadAsync();
                await page.EvaluateAsync("$find('ReportViewer1').exportReport('PDF');");

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