namespace CinemaControl.Configuration;

public record AppConfiguration
{
    public required string ApiToken { get; init; }
}