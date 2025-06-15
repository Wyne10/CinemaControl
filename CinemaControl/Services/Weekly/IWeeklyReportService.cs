namespace CinemaControl.Services.Weekly
{
    public interface IWeeklyReportService
    {
        Task<string> GetReportFilesAsync(DateTime startDate, DateTime endDate);
    }
} 