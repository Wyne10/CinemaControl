using Microsoft.Playwright;

namespace CinemaControl.Services;

public class CompositeReportService(IEnumerable<IReportService> reportServices) : ReportService
{
    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        foreach(IReportService reportService in reportServices) await reportService.GenerateReportFiles(from, to, page);
        return GetSessionPath(from, to);
    }
}