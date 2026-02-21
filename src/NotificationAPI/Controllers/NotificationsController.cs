using System.Threading.Channels;
using Application.Common.DTOs;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace NotificationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController(
    Channel<Notification> _channel, 
    ILogger<NotificationsController> _logger,
    NotificationService _notificationService
    ) : ControllerBase
{ 
    // ارسال اعلان‌ها به فرانت‌اند به‌صورت پشته‌ای (SSE)
    [HttpGet("stream")]
    public async IAsyncEnumerable<Notification> StreamNotifications()
    {
        _logger.LogInformation("Client connected to SSE stream.");

        while (await _channel.Reader.WaitToReadAsync())
        {
            if (_channel.Reader.TryRead(out var notification))
            {
                yield return notification;
            }
        }

        _logger.LogInformation("Client disconnected from SSE stream.");
    }

    // ارسال اعلان جدید (مثلاً از سرویس دیگر)
    [HttpPost]
    public async Task<IActionResult> AddNotification([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest("Message cannot be empty.");

        await _notificationService.AddNotificationAsync(message);
        return Ok();
    }
}
