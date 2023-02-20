namespace Brobot.Services;

public interface IGiphyService
{
    Task<string> GetGif(string? tag);
}