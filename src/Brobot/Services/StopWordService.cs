using Brobot.Repositories;

namespace Brobot.Services;

public class StopWordService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<StopWordService> logger) : IStopWordService
{
    private HashSet<string>? _stopWords;
    private bool _isOutdated = true;

    public async Task<bool> IsStopWord(string word)
    {
        logger.LogInformation("Checking stop word {Word}", word);
        if (_stopWords == null || _isOutdated)
        {
            logger.LogInformation("Getting stop words");
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var stopWordModels = await uow.StopWords.GetAll();
                _stopWords = stopWordModels.Select(sw => sw.Word).ToHashSet();
            }

            _isOutdated = false;
            logger.LogInformation("Finished getting stop words");
        }
        
        logger.LogInformation("Finished checking stop word {Word}", word);
        return _stopWords.Contains(word.ToLower());
    }

    public void StopWordsUpdated()
    {
        _isOutdated = true;
    }
}