namespace CinemaControl.Services
{
    public interface IWeeklyReportService
    {
        Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate);
    }
} 