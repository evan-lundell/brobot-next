using Brobot.Repositories;

namespace Brobot.Services;

public class StopWordService
{
    private readonly IServiceProvider _services;
    private HashSet<string>? _stopWords;
    private bool _isOutdated = true;

    public StopWordService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<bool> IsStopWord(string word)
    {
        if (_stopWords == null || _isOutdated)
        {
            using (var scope = _services.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var stopWordModels = await uow.StopWords.GetAll();
                _stopWords = stopWordModels.Select((sw) => sw.Word).ToHashSet();
            }

            _isOutdated = false;
        }

        return _stopWords.Contains(word);
    }

    public void StopWordsUpdated()
    {
        _isOutdated = true;
    }
}