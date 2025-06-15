using System.IO;

namespace CinemaControl.Services
{
    public class FakeWeeklyReportService : IWeeklyReportService
    {
        public async Task<IEnumerable<string>> GetReportFilesAsync(DateTime startDate, DateTime endDate)
        {
            var filePaths = new List<string>();
            var tempPath = Path.Combine(Path.GetTempPath(), "CinemaControlReports");
            Directory.CreateDirectory(tempPath);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var fileName = $"report_{date:yyyy-MM-dd}.pdf";
                var filePath = Path.Combine(tempPath, fileName);

                await Task.Run(() => File.WriteAllText(filePath, $"Это тестовый PDF отчет для {date:D}."));
                
                filePaths.Add(filePath);
            }

            return filePaths;
        }
    }
} 