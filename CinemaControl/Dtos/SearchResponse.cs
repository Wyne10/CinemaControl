using System.Text.Json.Serialization;

namespace CinemaControl.Dtos;

public class SearchResponse
{
    [JsonPropertyName("docs")]
    public List<Movie>? Docs { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }
}

public class Movie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("alternativeName")]
    public string? AlternativeName { get; set; }
        
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }
    
    [JsonPropertyName("movieLength")]
    public int? Length { get; set; } 
    
    [JsonPropertyName("ageRating")]
    public int? AgeRating { get; set; } 
        
    [JsonPropertyName("logo")]
    public Logo? Logo { get; set; }
    
    [JsonPropertyName("poster")]
    public Poster? Poster { get; set; }

    [JsonPropertyName("rating")]
    public Rating? Rating { get; set; }

    [JsonPropertyName("genres")]
    public List<Genre>? Genres { get; set; }

    [JsonPropertyName("countries")]
    public List<Country>? Countries { get; set; }
    
    public bool IsRussian() => Countries?.Any(c => c.Name == "Россия") ?? false;

    public bool IsChildrenAvailable() => AgeRating <= 6;
}

public class Logo
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class Poster
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }
}

public class Rating
{
    [JsonPropertyName("kp")]
    public double? Kp { get; set; }
        
    [JsonPropertyName("imdb")]
    public double? Imdb { get; set; }
    
    [JsonPropertyName("tmdb")]
    public double? Tmdb { get; set; }
    
    [JsonPropertyName("filmCritics")]
    public double? FilmCritics { get; set; }
    
    [JsonPropertyName("russianFilmCritics")]
    public double? RussianFilmCritics { get; set; }
    
    [JsonPropertyName("await")]
    public double? Await { get; set; }
}
    
public class Genre
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class Country
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}