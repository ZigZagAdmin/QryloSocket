using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Hubs;
using Serilog;
using StackExchange.Redis;
using Exception = System.Exception;

namespace QryloSocketAPI.Services.Implementation;

public class CachingService<T> : ICachingService<T> where T : class
{
    private readonly IDatabase _redisDatabase;
    private readonly IHubContext<SocketHub> _hubContext;

    public CachingService(IConnectionMultiplexer redisConnection, IHubContext<SocketHub> hubContext)
    {
        _hubContext = hubContext;
        try
        {
            _redisDatabase = redisConnection.GetDatabase();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not create Redis DB");
        }
    }
    
    public void DisconnectionListener()
    {
        try
        {
            if (_redisDatabase != null)
                _redisDatabase.Multiplexer.ConnectionFailed += async (sender, args) =>
                {
                    if (_hubContext != null && _hubContext.Clients != null) await _hubContext.Clients.All.SendAsync("DisconnectUser");
                    
                };
            
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not disconnect Redis DB");
        }
    }

    public void ReconnectionListener(ConcurrentDictionary<string, List<string>> topics)
    {
        if(_redisDatabase != null)
            _redisDatabase.Multiplexer.ConnectionRestored += async (sender, args) =>
            { 
                await _hubContext.Clients.All.SendAsync("DisconnectUser");
                topics.Clear();
            };
    }

    public bool IsRedisConnected()
    {
        return _redisDatabase is { Multiplexer.IsConnected: true };
    }

    public async Task<T> GetOrAddAsync(string cacheKey, Func<Task<T>> query, TimeSpan expiration)
    {
        if (_redisDatabase.Multiplexer.IsConnected)
        {
            var keyLock = await _redisDatabase.StringGetAsync(cacheKey + ":lock");
            var redisCacheData = await _redisDatabase.StringGetAsync(cacheKey);
            if (keyLock != "locked" && redisCacheData.HasValue)
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)redisCacheData.ToString().Replace("\\", string.Empty).Replace("\"", string.Empty);
                } 
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(redisCacheData);
            }
            if(keyLock == "locked" && !redisCacheData.HasValue) await _redisDatabase.KeyDeleteAsync(cacheKey + ":lock");
        }

        var result = await query();
        if (result != null)
        {
            if (_redisDatabase.Multiplexer.IsConnected)
            {
                var keyLock = await _redisDatabase.StringGetAsync(cacheKey + ":lock");
                if(keyLock != "locked" || (keyLock == "locked" && (await _redisDatabase.StringGetAsync(cacheKey)).IsNull))
                    await _redisDatabase.StringSetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(result), expiration);
            }
        }
        
        return result;
    }

    public async Task InvalidateCacheAsync(string cacheKey)
    {
        if (_redisDatabase.Multiplexer.IsConnected)
        {
            await _redisDatabase.KeyDeleteAsync(cacheKey);
        }
    }

    public async Task UpsertAsync(string cacheKey, T cacheObject, TimeSpan? expiry = null)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;
        await _redisDatabase.StringSetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheObject), expiry.HasValue ? new Expiration(expiry.Value) : default);
    }
    
    public async Task RemoveByPatternAsync(string pattern)
    {
        if (!_redisDatabase.Multiplexer.IsConnected) return;
        
        var keysToDelete = ((RedisResult[])await _redisDatabase.ExecuteAsync("KEYS", pattern) ?? []).Select(x => (RedisKey)(string)x).ToArray();
        await _redisDatabase.KeyDeleteAsync(keysToDelete);
    }

    public async Task<HashSet<string>> GetByPatternAsync(string pattern)
    {
        if (!_redisDatabase.Multiplexer.IsConnected) return null;

        var result = await _redisDatabase.ExecuteAsync("KEYS", pattern);
        return ((RedisResult[])result ?? []).Select(x => (string)x).ToHashSet();
    }

    public async Task SetKeys(HashSet<string> cacheKey)
    {
        if (!_redisDatabase.Multiplexer.IsConnected) return;

        var keysToAdd = cacheKey.Select(key => new KeyValuePair<RedisKey, RedisValue>(key, string.Empty)).ToArray();
        await _redisDatabase.StringSetAsync(keysToAdd, When.NotExists);
    }

    public async Task DeleteByPattern(string pattern)
    {
        if (!_redisDatabase.Multiplexer.IsConnected) return;
        
        var keysToDelete = ((RedisResult[])await _redisDatabase.ExecuteAsync("KEYS", pattern) ?? []).Select(x => (RedisKey)(string)x).ToArray();
        await _redisDatabase.KeyDeleteAsync(keysToDelete);
    }
}