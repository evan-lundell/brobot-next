namespace Brobot.Services;

public interface IDictionaryService
{
    Task<string> GetDefinition(string word);
}