using System.Text.Json.Serialization;
// ReSharper disable All

namespace CinemaControl.Dtos;

public record SearchResponse
{
    [JsonPropertyName("docs")]
    public required List<Movie> Docs { get; init; }

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("limit")]
    public int Limit { get; init; }

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pages")]
    public int Pages { get; init; }
}

public record Movie
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("alternativeName")]
    public required string AlternativeName { get; init; }
    
    [JsonPropertyName("enName")]
    public required string EnglishName { get; init; }
    
    [JsonPropertyName("type")]
    public required string Type { get; init; }
        
    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("shortDescription")]
    public required string ShortDescription { get; init; }
    
    [JsonPropertyName("movieLength")]
    public int Length { get; init; } 
    
    [JsonPropertyName("ageRating")]
    public int AgeRating { get; init; } 
        
    [JsonPropertyName("logo")]
    public required Logo Logo { get; init; }
    
    [JsonPropertyName("poster")]
    public required Poster Poster { get; init; }

    [JsonPropertyName("rating")]
    public required Rating Rating { get; init; }

    [JsonPropertyName("genres")]
    public required List<Genre> Genres { get; init; }

    [JsonPropertyName("countries")]
    public required List<Country> Countries { get; init; }
    
    [JsonPropertyName("isSeries")]
    public bool IsSeries { get; init; }  
    
    public bool IsRussian() => Countries?.Any(c => c.Name == "Россия") ?? false;

    public bool IsChildrenAvailable() => AgeRating <= 6;
}

public record Logo
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}

public record Poster
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; init; }
}

public record Video
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; } 
    [JsonPropertyName("site")]
    public string? Site { get; init; }  
    [JsonPropertyName("size")]
    public double? Size { get; init; } 
    [JsonPropertyName("type")]
    public double? Type { get; init; } 
}

public record Rating
{
    [JsonPropertyName("kp")]
    public double? Kp { get; init; }
        
    [JsonPropertyName("imdb")]
    public double? Imdb { get; init; }
    
    [JsonPropertyName("tmdb")]
    public double? Tmdb { get; init; }
    
    [JsonPropertyName("filmCritics")]
    public double? FilmCritics { get; init; }
    
    [JsonPropertyName("russianFilmCritics")]
    public double? RussianFilmCritics { get; init; }
    
    [JsonPropertyName("await")]
    public double? Await { get; init; }
}
    
public record Genre
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

public record Country
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}