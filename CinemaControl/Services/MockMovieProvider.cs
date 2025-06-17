namespace CinemaControl.Services;

public class MockMovieProvider : IMovieProvider
{
    public Task<bool> IsRussian(string movieName)
    {
        return Task.FromResult(Random.Shared.Next() % 2 == 0);
    }

    public Task<bool> IsChildrenAvailable(string movieName)
    {
        return Task.FromResult(Random.Shared.Next() % 2 == 0);
    }
}