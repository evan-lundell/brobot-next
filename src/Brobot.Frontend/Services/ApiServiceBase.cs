using System.Net.Http.Json;

namespace Brobot.Frontend.Services;

public abstract class ApiServiceBase<TRequest, TResponse, TKey>(
    string baseUrl,
    string entityName,
    HttpClient httpClient) : IApiService<TRequest, TResponse, TKey>
{
    protected string BaseUrl { get; } = baseUrl;
    protected HttpClient HttpClient { get; } = httpClient;
    protected string EntityName { get; } = entityName;

    protected async Task<IEnumerable<TResponse>> GetAll(string url)
    {
        var response = await HttpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TResponse>>() ?? [];
        }

        throw await response.ToException($"Failed to get data for entity {EntityName}");
    }

    public virtual Task<IEnumerable<TResponse>> GetAll()
        => GetAll(BaseUrl);

    protected async Task<TResponse> Get(string url)
    {
        var response = await HttpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() ??
                   throw new Exception($"Failed to parse entity {EntityName}");
        }
        
        throw await response.ToException($"Failed to get data for entity {EntityName}");
    }
    
    public virtual Task<TResponse> Get(TKey id)
        => Get($"{BaseUrl}/{id}");

    protected async Task<TResponse> Create(string url, TRequest request)
    {
        var response = await HttpClient.PostAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() ??
                   throw new Exception($"Failed to parse entity {EntityName}"); 
        }
        
        throw await response.ToException($"Failed to create entity {EntityName}");
    }

    public virtual Task<TResponse> Create(TRequest request)
        => Create(BaseUrl, request);
    
    protected async Task<TResponse> Update(string url, TRequest request)
    {
        var response = await HttpClient.PutAsJsonAsync(url, request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>() ??
                   throw new Exception($"Failed to parse entity {EntityName}"); 
        }
        
        throw await response.ToException($"Failed to update entity {EntityName}");
    }

    public virtual Task<TResponse> Update(TKey id, TRequest request)
        => Update($"{BaseUrl}/{id}", request);
    
    protected async Task Delete(string url)
    {
        var response = await HttpClient.DeleteAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        
        throw await response.ToException($"Failed to delete entity {EntityName}");
    }

    public virtual Task Delete(TKey id)
        => Delete($"{BaseUrl}/{id}");
}