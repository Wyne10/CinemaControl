using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public class SingleReportProvider(DateTime date) : ReportProvider
{
    private const string DateInputSelector = "input[name=\"ReportViewer1$ctl04$ctl05$txtValue\"]";
    
    public override async Task<IDownload> DownloadReport(IPage page, IFrame frame, ReportSaveType saveType)
    {
        var dateString = date.ToString("dd.MM.yyyy 0:00:00");
        await frame.FillAsync(DateInputSelector, dateString);
        
        return await base.DownloadReport(page, frame, saveType);
    }
}