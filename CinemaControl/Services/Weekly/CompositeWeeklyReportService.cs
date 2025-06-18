namespace CinemaControl.Services.Weekly;

public class CompositeWeeklyReportService(IEnumerable<IWeeklyReportService> weeklyReportServices) : CompositeReportService(weeklyReportServices), IWeeklyReportService
{
    public int GetFilesCount(DateTime from, DateTime to) =>
        weeklyReportServices
            .Select(w => w.GetFilesCount(from, to))
            .Aggregate((acc, next) => acc + next);
}