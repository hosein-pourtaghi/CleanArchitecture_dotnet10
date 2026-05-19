
// NotificationApi.Api/Services/NotificationSubscriptionManager.cs
using System.Collections.Concurrent;

namespace NotificationAPI.Services;

public interface INotificationSubscriptionManager
{
    Task AddConnectionAsync(string? userId, string connectionId);
    Task RemoveConnectionAsync(string? userId, string connectionId);
    IReadOnlyList<string> GetUserConnections(string userId);
    int GetTotalConnections();
}

public sealed class NotificationSubscriptionManager : INotificationSubscriptionManager
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, string> _connectionUsers = new();
    private int _totalConnections;

    public Task AddConnectionAsync(string? userId, string connectionId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            userId = $"anonymous:{connectionId}";
        }

        var connections = _userConnections.GetOrAdd(userId, _ => new HashSet<string>());
        lock (connections)
        {
            connections.Add(connectionId);
        }

        _connectionUsers.TryAdd(connectionId, userId);
        Interlocked.Increment(ref _totalConnections);

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string? userId, string connectionId)
    {
        if (!string.IsNullOrEmpty(userId) && _userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }

        _connectionUsers.TryRemove(connectionId, out _);
        Interlocked.Decrement(ref _totalConnections);

        return Task.CompletedTask;
    }

    public IReadOnlyList<string> GetUserConnections(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }
        return [];
    }

    public int GetTotalConnections()
    {
        return _totalConnections;
    }
}
