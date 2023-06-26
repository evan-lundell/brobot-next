using System.Net.Http.Json;
using Brobot.Shared.Responses;

namespace Brobot.Frontend.Services;

public abstract class ApiServiceBase<TRequest, TResponse, TKey> : IApiService<TRequest, TResponse, TKey>
{
    protected string BaseUrl { get; }
    protected HttpClient HttpClient { get; }
    protected string EntityName { get; }
    
    protected ApiServiceBase(string baseUrl, string entityName, HttpClient httpClient)
    {
        BaseUrl = baseUrl;
        EntityName = entityName;
        HttpClient = httpClient;
    }

    protected async Task<IEnumerable<TResponse>> GetAll(string url)
    {
        var response = await HttpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TResponse>>() ?? Array.Empty<TResponse>();
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? $"Failed to get data for entity {EntityName}");
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
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? $"Failed to get data for entity {EntityName}");
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
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? $"Failed to create entity {EntityName}");
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
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? $"Failed to update entity {EntityName}");
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
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        throw new Exception(errorResponse?.Title ?? $"Failed to delete entity {EntityName}");
    }

    public virtual Task Delete(TKey id)
        => Delete($"{BaseUrl}/{id}");
}