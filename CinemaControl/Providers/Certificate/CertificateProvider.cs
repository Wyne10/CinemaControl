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

    private async Task<Dictionary<string, string>> ParseTable(ILocator table)
    {
        var certificates = new Dictionary<string, string>();
        var rows = await table.Locator("tr").AllAsync();

        foreach (var row in rows.Skip(1))
        {
            var cells = await row.Locator("td").AllAsync();

            var movieName = await cells[ICertificateProvider.MovieNameColumnIndex].InnerTextAsync();
            var certificate = await cells[ICertificateProvider.CertificateColumnIndex].InnerTextAsync();
            certificates[movieName.Trim()] = certificate.Trim();
        }

        return certificates;
    }
}