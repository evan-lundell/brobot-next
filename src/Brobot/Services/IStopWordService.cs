namespace Brobot.Services;

public interface IStopWordService
{
    Task<bool> IsStopWord(string word);
    void StopWordsUpdated();
}