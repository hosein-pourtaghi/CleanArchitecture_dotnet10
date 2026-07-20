// NotificationApi.Api/Hubs/NotificationHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationAPI.Services;

namespace NotificationAPI.Hubs;

/// <summary>
/// SignalR hub for real-time notifications to Angular clients
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly INotificationSubscriptionManager _subscriptionManager;

    public NotificationHub(
        ILogger<NotificationHub> logger,
        INotificationSubscriptionManager subscriptionManager)
    {
        _logger = logger;
        _subscriptionManager = subscriptionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        var connectionId = Context.ConnectionId;

        _logger.LogInformation(
            "Client connected: {ConnectionId}, User: {UserId}",
            connectionId, userId);

        // Track connection
        await _subscriptionManager.AddConnectionAsync(userId, connectionId);

        // Add to default groups based on roles
        var roles = Context.User?.FindAll("role").Select(c => c.Value).ToList() ?? [];

        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(connectionId, $"role:{role}");
            _logger.LogDebug("Added connection {ConnectionId} to group: {Group}", connectionId, $"role:{role}");
        }

        // Add to admin group if applicable
        if (roles.Contains("Admin") || roles.Contains("ChecklistManager"))
        {
            await Groups.AddToGroupAsync(connectionId, "admins");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;

        _logger.LogInformation(
            "Client disconnected: {ConnectionId}, User: {UserId}, Exception: {Exception}",
            connectionId, userId, exception?.Message);

        await _subscriptionManager.RemoveConnectionAsync(userId, connectionId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific entity notifications (e.g., a specific checklist)
    /// </summary>
    public async Task SubscribeToEntity(string entityType, string entityId)
    {
        var groupName = $"entity:{entityType}:{entityId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to entity: {EntityType}:{EntityId}",
            Context.ConnectionId, entityType, entityId);
    }

    /// <summary>
    /// Unsubscribe from entity notifications
    /// </summary>
    public async Task UnsubscribeFromEntity(string entityType, string entityId)
    {
        var groupName = $"entity:{entityType}:{entityId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} unsubscribed from entity: {EntityType}:{EntityId}",
            Context.ConnectionId, entityType, entityId);
    }

    /// <summary>
    /// Subscribe to notifications for a specific user
    /// </summary>
    public async Task SubscribeToUser(string targetUserId)
    {
        var groupName = $"user:{targetUserId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to user notifications: {UserId}",
            Context.ConnectionId, targetUserId);
    }
}
