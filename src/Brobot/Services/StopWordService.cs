using Brobot.Repositories;

namespace Brobot.Services;

public class StopWordService(IServiceProvider services) : IStopWordService
{
    private HashSet<string>? _stopWords;
    private bool _isOutdated = true;

    public async Task<bool> IsStopWord(string word)
    {
        if (_stopWords == null || _isOutdated)
        {
            using (var scope = services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var stopWordModels = await uow.StopWords.GetAll();
                _stopWords = stopWordModels.Select(sw => sw.Word).ToHashSet();
            }

            _isOutdated = false;
        }
        
        return _stopWords.Contains(word.ToLower());
    }

    public void StopWordsUpdated()
    {
        _isOutdated = true;
    }
}