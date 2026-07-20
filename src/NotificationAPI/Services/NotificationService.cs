// NotificationApi.Api/Services/NotificationService.cs
using Microsoft.AspNetCore.SignalR;
using NotificationAPI.Dtos;
using NotificationAPI.Hubs;

namespace NotificationAPI.Services;

public interface INotificationService
{
    Task BroadcastAsync(DomainEventNotificationDto notification, CancellationToken cancellationToken = default);
    Task SendToUserAsync(string userId, DomainEventNotificationDto notification, CancellationToken cancellationToken = default);
    Task SendToGroupAsync(string groupName, DomainEventNotificationDto notification, CancellationToken cancellationToken = default);
}

public sealed class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastAsync(
        DomainEventNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Broadcasting notification: {EventType} to all clients",
            notification.EventType);

        await _hubContext.Clients.All.SendAsync(
            "ReceiveNotification",
            notification,
            cancellationToken);
    }

    public async Task SendToUserAsync(
        string userId,
        DomainEventNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Sending notification to user {UserId}: {EventType}",
            userId, notification.EventType);

        await _hubContext.Clients.Group($"user:{userId}").SendAsync(
            "ReceiveNotification",
            notification,
            cancellationToken);
    }

    public async Task SendToGroupAsync(
        string groupName,
        DomainEventNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Sending notification to group {Group}: {EventType}",
            groupName, notification.EventType);

        await _hubContext.Clients.Group(groupName).SendAsync(
            "ReceiveNotification",
            notification,
            cancellationToken);
    }
}
