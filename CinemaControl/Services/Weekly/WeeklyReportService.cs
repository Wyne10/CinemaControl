using System.Windows.Controls;

namespace CinemaControl.Services.Weekly;

public abstract class WeeklyReportService(ProgressBar progressBar) : ReportService, IWeeklyReportService
{
    protected ProgressBar ProgressBar { get; private set; } = progressBar;

    public abstract int GetFilesCount(DateTime from, DateTime to);
}