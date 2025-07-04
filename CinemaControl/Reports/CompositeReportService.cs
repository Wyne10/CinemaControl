﻿using Microsoft.Playwright;

namespace CinemaControl.Reports;

public class CompositeReportService : ReportService
{
    private readonly IEnumerable<IReportService> _reportServices;
    
    public CompositeReportService(IEnumerable<IReportService> reportServices)
    {
        _reportServices = reportServices;
        foreach (var reportService in _reportServices) reportService.OnDownloadProgress += ProgressDownload;
    }
    
    public override async Task<string> GenerateReportFiles(DateTime from, DateTime to, IPage page)
    {
        foreach(var reportService in _reportServices) await reportService.GenerateReportFiles(from, to, page);
        return GetSessionPath(from, to);
    }
}