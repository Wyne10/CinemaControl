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
        var certificates = await table.EvaluateAsync<Dictionary<string, string>>(@"
        (table) => {
            const result = {};
            const rows = Array.from(table.querySelectorAll('tr')).slice(1); // skip header

            for (const row of rows) {
                const cells = row.querySelectorAll('td');
                const movie = cells[" + ICertificateProvider.MovieNameColumnIndex + @"]?.innerText.trim();
                const cert = cells[" + ICertificateProvider.CertificateColumnIndex + @"]?.innerText.trim();
                if (movie && cert) {
                    result[movie] = cert;
                }
            }
            return result;
        }
    ");

        return certificates ?? new Dictionary<string, string>();
    }
}