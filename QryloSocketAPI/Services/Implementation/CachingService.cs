using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Hubs;
using QryloSocketAPI.Models;
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

    public async Task<T> GetAsync(string cacheKey)
    {
        if (!(_redisDatabase is { Multiplexer.IsConnected: true })) return null;
        var redisCacheData = await _redisDatabase.StringGetAsync(cacheKey);
        if (!redisCacheData.HasValue) return null;
        if (typeof(T) == typeof(string))
        {
            return (T)(object)redisCacheData.ToString().Replace("\\", string.Empty).Replace("\"", string.Empty);
        } 
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(redisCacheData);
    }

    public async Task UpsertAsync(string cacheKey, T cacheObject, TimeSpan? expiry = null)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;
        await _redisDatabase.StringSetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(cacheObject), expiry.HasValue ? new Expiration(expiry.Value) : default);
    }

    public async Task RemoveAsync(string cacheKey)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;
        await _redisDatabase.KeyDeleteAsync(cacheKey);
    }

    public async Task<Dictionary<string, HashSet<string>>> GetConnectedUsersAsync(string companyId)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{companyId}:*").ToList();

        var connections = new HashSet<string>();
        var userIds = new HashSet<string>();

        foreach (var match in keys.Select(key => Regex.Match(key.ToString(), $"^{companyId}:(.+):(.+)$")).Where(match => match.Success))
        {
            connections.Add(match.Groups[1].Value);
            userIds.Add(match.Groups[2].Value);
        }

        var result = new Dictionary<string, HashSet<string>>
        {
            { "connections", connections.Distinct().ToHashSet() },
            { "userIds", userIds.Distinct().ToHashSet() }
        };

        return result;
    }

    public async Task<List<UserConnection>> GetConnectedUsersPerConnectionAsync(string companyId)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{companyId}:*").ToList();

        var connectedUsers = new List<UserConnection>();

        foreach (var match in keys.Select(key => Regex.Match(key.ToString(), $"^{companyId}:(.+):(.+)$")).Where(match => match.Success))
        {
            connectedUsers.Add(new UserConnection
            {
                ConnectionId = match.Groups[1].Value,
                UserId = match.Groups[2].Value 
            });
        }

        return connectedUsers;
    }

    public async Task DeleteTopicsConnectionAsync(string connectionId)
    {
        if (_redisDatabase == null) return;
        var values = typeof(Topics).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)?.ToString())
            .ToList();
        foreach (var value in values)
        {
            await _redisDatabase.KeyDeleteAsync("topics:" + value + ":" + connectionId);
        }
    }

    public async Task<HashSet<long>> GetListAsync(string cacheKey)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return new HashSet<long>();

        var values = await _redisDatabase.ListRangeAsync(cacheKey);
        return values.Select(v => long.Parse(v)).ToHashSet();
    }

    public async Task AppendToListAsync(string cacheKey, long value)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;

        await _redisDatabase.ListRightPushAsync(cacheKey, value.ToString());
    }

    public async Task RemoveFromListAsync(string cacheKey, long value)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;
        await _redisDatabase.ListRemoveAsync(cacheKey, value.ToString(), 1);
    }

    public async Task AppendListToListAsync(string cacheKey, HashSet<long> values, TimeSpan? expiry = null)
    {
        if (_redisDatabase is not { Multiplexer.IsConnected: true }) return;
    
        if (values == null || values.Count == 0) return;
    
        await _redisDatabase.ListRightPushAsync(cacheKey, values.Select(v => (RedisValue)v).ToArray());
        
        if (expiry.HasValue)
        {
            await _redisDatabase.KeyExpireAsync(cacheKey, expiry);
        }
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
}