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
        var script = $@"
        (table) => {{
            const result = {{}};
            const rows = table.querySelectorAll('tbody tr');

            for (let i = 1; i < rows.length; i++) {{ // skip header row
                const cells = rows[i].querySelectorAll('td');

                const movie = cells[{ICertificateProvider.MovieNameColumnIndex}]?.innerText?.trim();
                const cert = cells[{ICertificateProvider.CertificateColumnIndex}]?.innerText?.trim();

                if (movie && cert) {{
                    result[movie] = cert;
                }}
            }}

            return result;
        }}
    ";
        
        var certificates = await table.EvaluateAsync<Dictionary<string, string>>(script);
        return certificates ?? new Dictionary<string, string>();
    }
}