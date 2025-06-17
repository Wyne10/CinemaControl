using System.Windows.Controls;
using Microsoft.Playwright;

namespace CinemaControl.Services.Weekly;

public abstract class WeeklyReportService(ProgressBar progressBar) : ReportService, IWeeklyReportService
{
    protected ProgressBar ProgressBar { get; private set; } = progressBar;

    public abstract int GetFilesCount(DateTime startDate, DateTime endDate);

    public abstract Task<string> GetReportFiles(DateTime startDate, DateTime endDate, IPage page);
}