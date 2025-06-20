using Microsoft.Playwright;

namespace CinemaControl.Services;

public class CompositeReportService : ReportService
{
    private readonly IEnumerable<IReportService> _reportServices;
    
    public CompositeReportService(IEnumerable<IReportService> reportServices)
    {
        _reportServices = reportServices;
        foreach (IReportService reportService in _reportServices) reportService.OnDownloadProgress += ProgressDownload;
    }
    
    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        foreach (IReportService reportService in _reportServices) await reportService.GenerateReportFiles(from, to, page);
        return GetSessionPath(from, to);
    }
}