using Microsoft.Playwright;

namespace CinemaControl.Providers.Certificate;

public class CertificateProvider : ICertificateProvider
{
    public async Task<Dictionary<string, string>> GetCertificates(IPage page, IEnumerable<string> movieNames)
    {
        var certificates = new Dictionary<string, string>();
        
        await page.GotoAsync(ICertificateProvider.MovieArchiveUrl);
        var tableLocator = page.Locator(ICertificateProvider.TableSelector).Nth(2);
        
        foreach (var certificate in await ParseTable(tableLocator))
            certificates[certificate.Key] = certificate.Value;
        
        await page.ClickAsync(ICertificateProvider.ArchiveButtonSelector);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        foreach (var certificate in await ParseTable(tableLocator))
            certificates[certificate.Key] = certificate.Value;
        
        certificates
            .Where(movie => !movieNames.Contains(movie.Key))
            .ToList()
            .ForEach(movie => certificates.Remove(movie.Key));
        
        return certificates;
    }

    private static async Task<Dictionary<string, string>> ParseTable(ILocator table)
    {
        var certificates = new Dictionary<string, string>();

        var movieNames = await table.Locator($"tr:not(:first-child) td:nth-child({ICertificateProvider.MovieNameColumnIndex + 1})").AllInnerTextsAsync();
        var certificateValues = await table.Locator($"tr:not(:first-child) td:nth-child({ICertificateProvider.CertificateColumnIndex + 1})").AllInnerTextsAsync();
        
        for (var i = 0; i < movieNames.Count; i++)
        {
            certificates[movieNames[i].Trim()] = certificateValues[i].Trim();
        }

        return certificates;
    }
}