using CinemaControl.Providers.Movie;
using Microsoft.Playwright;

namespace CinemaControl.Services.Monthly;

public class MonthlyReportService(IMovieProvider movieProvider) : ReportService, IMonthlyReportService
{
    private const string ReportUrl = "http://192.168.0.254/CinemaWeb/Report/Render?path=RentalReports%2FGrossMovieByPeriodn";

    public Task<string> GenerateReportFile(DateTime date, IPage page)
    {
        
    }

}