namespace CinemaControl.Providers.Report;

public record ReportSaveType(string Selector)
{
    public static readonly ReportSaveType Pdf = new("a[title=\"PDF\"]"); 
    public static readonly ReportSaveType Word = new("a[title=\"Word\"]"); 
    public static readonly ReportSaveType Excel = new("a[title=\"Excel\"]"); 
}