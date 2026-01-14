using System.Collections.Concurrent;
using QryloSocketAPI.Models;

namespace QryloSocketAPI.Services;

public interface ICachingService<T> where T : class
{
    public void DisconnectionListener();

    public void ReconnectionListener(ConcurrentDictionary<string, List<string>> topics);
   
    public bool IsRedisConnected();

    public Task<T> GetOrAddAsync(string cacheKey, Func<Task<T>> query, TimeSpan expiration);
    
    public Task<T> GetAsync(string cacheKey);
    
    public Task UpsertAsync(string cacheKey, T cacheObject, TimeSpan? expiry = null);

    public Task RemoveAsync(string cacheKey);
    
    public Task<Dictionary<string, HashSet<string>>> GetConnectedUsersAsync(string companyId);

    public Task<List<UserConnection>> GetConnectedUsersPerConnectionAsync(string companyId);
    
    public Task DeleteTopicsConnectionAsync(string connectionId);
    
    public Task<HashSet<long>> GetListAsync(string cacheKey);

    public Task AppendToListAsync(string cacheKey, long value);

    public Task RemoveFromListAsync(string cacheKey, long value);

    public Task AppendListToListAsync(string cacheKey, HashSet<long> values, TimeSpan? expiry = null);
    
    public Task RemoveByPatternAsync(string pattern);
    
    public Task<HashSet<string>> GetByPatternAsync(string pattern);
}