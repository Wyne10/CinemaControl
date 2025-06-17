namespace CinemaControl.Services;

public interface IMovieProvider
{
    Task<bool> IsRussian(string movieName);
    Task<bool> IsChildrenAvailable(string movieName);
}