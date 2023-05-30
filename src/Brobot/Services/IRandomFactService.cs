namespace Brobot.Services;

public interface IRandomFactService
{
    Task<string> GetFact();
}