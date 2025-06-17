using Microsoft.Playwright;

namespace CinemaControl.Providers.Report;

public interface IReportProvider
{
    public const string ViewReportButtonSelector = "input[name=\"ReportViewer1$ctl04$ctl00\"]";
    public const string ExportMenuLinkSelector = "a#ReportViewer1_ctl05_ctl04_ctl00_ButtonLink";

    Task<IDownload> DownloadReport(IPage page, IFrame frame, ReportSaveType saveType);
}