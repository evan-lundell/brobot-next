namespace Brobot.Services;

public interface IStopWordService
{
    Task<bool> IsStopWord(string word, CancellationToken cancellationToken = default);
    void StopWordsUpdated();
}