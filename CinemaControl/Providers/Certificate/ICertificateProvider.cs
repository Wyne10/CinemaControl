using Microsoft.Playwright;

namespace CinemaControl.Providers.Certificate;

public interface ICertificateProvider
{
    public const string MovieArchiveUrl = "http://192.168.0.254/CinemaWeb/Movie";
    public const string ArchiveButtonSelector = "input[value=\"InArchive\"]";
    public const string TableSelector = "table";
    public const int MovieNameColumnIndex = 0;
    public const int CertificateColumnIndex = 7;
    
    Task<Dictionary<string, string>> GetCertificates(IPage page, IEnumerable<string> movieNames);
}