namespace Brobot.Frontend.Services;

public interface IApiService<in TRequest, TResponse, in TKey>
{
    Task<IEnumerable<TResponse>> GetAll();
    Task<TResponse> Get(TKey id);
    Task<TResponse> Create(TRequest request);
    Task<TResponse> Update(TKey id, TRequest request);
    Task Delete(TKey id);
}