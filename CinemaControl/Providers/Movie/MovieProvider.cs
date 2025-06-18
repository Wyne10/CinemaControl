using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using CinemaControl.Dtos;
using CinemaControl.Services;

namespace CinemaControl.Providers.Movie;

public class MovieProvider : IMovieProvider
{
    private const string ApiBaseUrl = "https://api.kinopoisk.dev/v1.4/movie/search";
    private static readonly HttpClient HttpClient = new();
    
    private readonly SettingsService _settingsService;
    
    private readonly string _movieCacheFilePath;
    private readonly Dictionary<string, Dtos.Movie> _movieCache;

    public MovieProvider(SettingsService settingsService)
    {
        _settingsService = settingsService;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolderPath = Path.Combine(appDataPath, "CinemaControl");
        Directory.CreateDirectory(appFolderPath);
        _movieCacheFilePath = Path.Combine(appFolderPath, "movies.json");
            
        _movieCache = LoadMovieCache(); 
    }

    public async Task<bool> IsRussian(string movieName) =>
        (_movieCache!.GetValueOrDefault(movieName, WriteMovieCache(movieName, await FetchMovieData(movieName)))?.Countries ?? Enumerable.Empty<Country>()).Any(c => c.Name == "Россия");

    public async Task<bool> IsChildrenAvailable(string movieName) =>
        _movieCache!.GetValueOrDefault(movieName, WriteMovieCache(movieName, await FetchMovieData(movieName)))?.AgeRating <= 6;

    public async Task<Dictionary<string, Dtos.Movie>> GetMovies(IEnumerable<string> movieNames)
    {
        var movies = new Dictionary<string, Dtos.Movie>();
        foreach (var movieName in movieNames)
        {
            var movie = _movieCache!.GetValueOrDefault(movieName, WriteMovieCache(movieName, await FetchMovieData(movieName)));
            if (movie != null)
                movies[movieName] = movie;
        }
        return movies;
    }

    private async Task<Dtos.Movie?> FetchMovieData(string movieName)
    {
        var apiToken = _settingsService.Settings.ApiToken;
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            throw new Exception("Не установлен API токен.");
        }

        var builder = new UriBuilder(ApiBaseUrl);
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["page"] = "1";
        query["limit"] = "1";
        query["query"] = movieName;
        builder.Query = query.ToString();
            
        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-API-KEY", apiToken);

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<SearchResponse>(jsonResponse);

        var movieDto = searchResult?.Docs?.FirstOrDefault();
        if (movieDto == null)
        {
            throw new Exception($"Фильм {movieName} не найден.");
        }

        return movieDto;
    }

    private Dtos.Movie? WriteMovieCache(string movieName, Dtos.Movie? movie)
    {
        if (movie == null)
            return movie; 
        _movieCache[movieName] = movie;
        SaveMovieCache();
        return movie;
    }
    
    private Dictionary<string, Dtos.Movie> LoadMovieCache()
    {
        if (!File.Exists(_movieCacheFilePath))
        {
            return new Dictionary<string, Dtos.Movie>();
        }

        try
        {
            var json = File.ReadAllText(_movieCacheFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, Dtos.Movie>>(json) ?? new Dictionary<string, Dtos.Movie>();
        }
        catch
        {
            // If file is corrupt or invalid, return default settings
            return new Dictionary<string, Dtos.Movie>();
        }
    }

    private async void SaveMovieCache()
    {
        var json = JsonSerializer.Serialize(_movieCache, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_movieCacheFilePath, json);
    }
}