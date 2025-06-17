namespace CinemaControl.Services.Weekly;

public interface IWeeklyReportService : IReportService
{
    int GetFilesCount(DateTime from, DateTime to);
}