namespace CinemaControl.Services
{
    public interface IWeeklyReportService
    {
        Task<IEnumerable<string>> GetReportFilesAsync(DateTime startDate, DateTime endDate);
    }
} 