using Microsoft.Playwright;

namespace CinemaControl.Services;

public class CompositeReportService(IEnumerable<IReportService> reportServices) : ReportService
{
    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        foreach (IReportService reportService in reportServices)
        {
            reportService.OnDownloadProgress += ProgressDownload;
            await reportService.GenerateReportFiles(from, to, page);
            reportService.OnDownloadProgress -= ProgressDownload;
        }
        return GetSessionPath(from, to);
    }
}