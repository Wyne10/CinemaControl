namespace CinemaControl.Providers.Movie;

public interface IMovieProvider
{
    Task<Dictionary<string, Dtos.Movie>> GetMovies(IEnumerable<string> movieNames);
}