using System.Threading.Channels;
using Application.Common.DTOs;

namespace Infrastructure.Services;

public class NotificationService(Channel<Notification> _channel)
{
    public async Task AddNotificationAsync(string message)
    {
        await _channel.Writer.WriteAsync(new Notification { Message = message });
    }
}
