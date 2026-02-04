namespace QryloSocketAPI.Services;

public interface ICachingService<T> where T : class
{
    public bool IsRedisConnected();

    public Task<T> GetOrAddAsync(string cacheKey, Func<Task<T>> query, TimeSpan expiration);

    public Task InvalidateCacheAsync(string cacheKey);
    
    public Task UpsertAsync(string cacheKey, T cacheObject, TimeSpan? expiry = null);
    
    public Task RemoveByPatternAsync(string pattern);
    
    public Task<HashSet<string>> GetByPatternAsync(string pattern);
    
    public Task SetKeys(HashSet<string> cacheKey);

    public Task DeleteByPattern(string pattern);
}